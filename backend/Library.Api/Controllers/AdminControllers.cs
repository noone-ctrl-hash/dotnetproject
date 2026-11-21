using Library.Api.Data;
using Library.Api.Dtos;
using Library.Api.Models;
using Library.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Library.Api.Controllers;

// ============ Admin : statistiques + modération des contenus ============
[Authorize(Roles = "Admin,SuperAdmin"), ApiController, Route("api/admin")]
public class AdminController(AppDbContext db) : ControllerBase
{
    /// <summary>Consulter les statistiques (données des graphiques Chart.js).</summary>
    [HttpGet("stats")]
    public async Task<IActionResult> Stats()
    {
        var today = DateTime.UtcNow.Date;
        var from = today.AddDays(-29);

        // Projections simples puis agrégations en mémoire (volumes modestes, traduction SQL sans surprise).
        var purchases = await db.Purchases
            .Select(p => new
            {
                p.PurchasedAt, p.Amount, p.Status, p.BookId, p.UserId,
                Title = p.Book!.Title, Category = p.Book!.Category!.Name
            }).ToListAsync();
        var success = purchases.Where(p => p.Status == PurchaseStatus.Success).ToList();

        var revenueByDay = Enumerable.Range(0, 30).Select(i => from.AddDays(i)).Select(d => new
        {
            date = d.ToString("yyyy-MM-dd"),
            revenue = success.Where(p => p.PurchasedAt.Date == d).Sum(p => p.Amount),
            sales = success.Count(p => p.PurchasedAt.Date == d)
        }).ToList();

        var byCategory = success.GroupBy(p => p.Category)
            .Select(g => new { category = g.Key, revenue = g.Sum(p => p.Amount), sales = g.Count() })
            .OrderByDescending(x => x.revenue).ToList();

        var topBooks = success.GroupBy(p => p.Title)
            .Select(g => new { title = g.Key, sales = g.Count(), revenue = g.Sum(p => p.Amount) })
            .OrderByDescending(x => x.sales).Take(6).ToList();

        var byStatus = purchases.GroupBy(p => p.Status)
            .Select(g => new { status = g.Key.ToString(), count = g.Count() }).ToList();

        var userDates = await db.Users.Select(u => u.RegisteredAt).ToListAsync();
        var firstMonth = new DateTime(today.Year, today.Month, 1).AddMonths(-5);
        var newUsers = Enumerable.Range(0, 6).Select(i => firstMonth.AddMonths(i)).Select(m => new
        {
            month = m.ToString("yyyy-MM"),
            count = userDates.Count(d => d.Year == m.Year && d.Month == m.Month)
        }).ToList();

        var books = await db.Books.Where(b => b.Status == BookStatus.Published).Select(b => b.IsFree).ToListAsync();

        return Ok(new
        {
            totals = new
            {
                users = await db.Users.CountAsync(),
                books = books.Count,
                freeBooks = books.Count(f => f),
                paidBooks = books.Count(f => !f),
                sales = success.Count,
                revenue = success.Sum(p => p.Amount),
                publications = await db.Publications.CountAsync(p => p.Status == ContentStatus.Published),
                comments = await db.Comments.CountAsync(c => c.Status == ContentStatus.Published),
                openReports = await db.Reports.CountAsync(r => r.Status == ReportStatus.Open || r.Status == ReportStatus.UnderReview)
            },
            revenueByDay, byCategory, topBooks, byStatus, newUsers
        });
    }

    // ----- Modérer les contenus -----
    [HttpGet("publications")]
    public async Task<IEnumerable<PublicationDto>> Publications()
    {
        var items = await db.Publications.Include(p => p.User).Include(p => p.Book)
            .Where(p => p.Status != ContentStatus.Deleted).OrderByDescending(p => p.PublishedAt).ToListAsync();
        return items.Select(p => p.ToDto(0));
    }

    [HttpPut("publications/{id:int}/status")]
    public async Task<IActionResult> SetPublicationStatus(int id, StatusRequest r)
    {
        var p = await db.Publications.FindAsync(id);
        if (p is null) return NotFound();
        p.Status = r.Status;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("comments")]
    public async Task<IEnumerable<CommentDto>> Comments() =>
        (await db.Comments.Include(c => c.User).Where(c => c.Status != ContentStatus.Deleted)
            .OrderByDescending(c => c.CommentedAt).Take(200).ToListAsync()).Select(c => c.ToDto());

    [HttpPut("comments/{id:int}/status")]
    public async Task<IActionResult> SetCommentStatus(int id, StatusRequest r)
    {
        var c = await db.Comments.FindAsync(id);
        if (c is null) return NotFound();
        c.Status = r.Status;
        await db.SaveChangesAsync();
        return NoContent();
    }
}

// ============ Super administrateur : utilisateurs, rôles, administrateurs ============
[Authorize(Roles = "SuperAdmin"), ApiController, Route("api/users")]
public class UsersController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IEnumerable<UserDto>> List(UserRole? role)
    {
        var q = db.Users.AsQueryable();
        if (role.HasValue) q = q.Where(u => u.Role == role.Value);
        return (await q.OrderBy(u => u.FullName).ToListAsync()).Select(u => u.ToDto());
    }

    /// <summary>Gérer les rôles et permissions.</summary>
    [HttpPut("{id:int}/role")]
    public async Task<ActionResult<UserDto>> SetRole(int id, RoleRequest r)
    {
        if (id == User.UserId()) return BadRequest(new { message = "Vous ne pouvez pas modifier votre propre rôle." });
        var u = await db.Users.FindAsync(id);
        if (u is null) return NotFound();
        u.Role = r.Role;
        await db.SaveChangesAsync();
        return u.ToDto();
    }

    /// <summary>Gérer les administrateurs : création d'un compte administrateur.</summary>
    [HttpPost("admins")]
    public async Task<ActionResult<UserDto>> CreateAdmin(CreateAdminRequest r)
    {
        var email = r.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(u => u.Email == email)) return Conflict(new { message = "Un compte existe déjà avec cet e-mail." });
        var u = new User { FullName = r.FullName.Trim(), Email = email, Role = UserRole.Admin, PasswordHash = BCrypt.Net.BCrypt.HashPassword(r.Password) };
        db.Users.Add(u);
        await db.SaveChangesAsync();
        return u.ToDto();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (id == User.UserId()) return BadRequest(new { message = "Vous ne pouvez pas supprimer votre propre compte." });
        var u = await db.Users.FindAsync(id);
        if (u is null) return NotFound();
        try
        {
            db.Users.Remove(u);
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict(new { message = "Suppression impossible : du contenu lié à cet utilisateur est référencé ailleurs." });
        }
        return NoContent();
    }
}
