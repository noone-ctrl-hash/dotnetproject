using Library.Api.Data;
using Library.Api.Dtos;
using Library.Api.Models;
using Library.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Library.Api.Controllers;

// ============ Publications : publier / modifier / supprimer une réflexion ou un avis ============
[ApiController, Route("api/publications")]
public class PublicationsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IEnumerable<PublicationDto>> List(int? bookId, string? type, bool mine = false)
    {
        var q = db.Publications.Include(p => p.User).Include(p => p.Book)
            .Where(p => p.Status == ContentStatus.Published);
        if (bookId.HasValue) q = q.Where(p => p.BookId == bookId.Value);
        if (!string.IsNullOrWhiteSpace(type)) q = q.Where(p => p.Type == type);
        if (mine && User.Identity?.IsAuthenticated == true)
        {
            var uid = User.UserId();
            q = q.Where(p => p.UserId == uid);
        }
        var items = await q.OrderByDescending(p => p.PublishedAt).ToListAsync();
        var ids = items.Select(p => p.Id).ToList();
        var counts = await db.Comments
            .Where(c => ids.Contains(c.PublicationId) && c.Status == ContentStatus.Published)
            .GroupBy(c => c.PublicationId).Select(g => new { g.Key, N = g.Count() }).ToDictionaryAsync(x => x.Key, x => x.N);
        return items.Select(p => p.ToDto(counts.GetValueOrDefault(p.Id)));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PublicationDto>> Get(int id)
    {
        var p = await db.Publications.Include(x => x.User).Include(x => x.Book).FirstOrDefaultAsync(x => x.Id == id);
        if (p is null || p.Status != ContentStatus.Published) return NotFound();
        var n = await db.Comments.CountAsync(c => c.PublicationId == id && c.Status == ContentStatus.Published);
        return p.ToDto(n);
    }

    [Authorize, HttpPost]
    public async Task<ActionResult<PublicationDto>> Publish(PublicationRequest r)
    {
        if (r.BookId.HasValue && !await db.Books.AnyAsync(b => b.Id == r.BookId.Value))
            return BadRequest(new { message = "Livre introuvable." });
        var p = new Publication
        {
            Title = r.Title.Trim(), Content = r.Content.Trim(), BookId = r.BookId,
            Type = r.Type == "Avis" ? "Avis" : "Reflexion", UserId = User.UserId(), PublishedAt = DateTime.UtcNow
        };
        db.Publications.Add(p);
        await db.SaveChangesAsync();
        await db.Entry(p).Reference(x => x.User).LoadAsync();
        await db.Entry(p).Reference(x => x.Book).LoadAsync();
        return p.ToDto(0);
    }

    [Authorize, HttpPut("{id:int}")]
    public async Task<ActionResult<PublicationDto>> Modify(int id, PublicationRequest r)
    {
        var p = await db.Publications.Include(x => x.User).Include(x => x.Book).FirstOrDefaultAsync(x => x.Id == id);
        if (p is null || p.Status == ContentStatus.Deleted) return NotFound();
        if (p.UserId != User.UserId()) return Forbid();
        p.Title = r.Title.Trim(); p.Content = r.Content.Trim();
        p.Type = r.Type == "Avis" ? "Avis" : "Reflexion";
        await db.SaveChangesAsync();
        var n = await db.Comments.CountAsync(c => c.PublicationId == id && c.Status == ContentStatus.Published);
        return p.ToDto(n);
    }

    [Authorize, HttpDelete("{id:int}")]
    public async Task<IActionResult> Remove(int id)
    {
        var p = await db.Publications.FindAsync(id);
        if (p is null) return NotFound();
        if (p.UserId != User.UserId() && !User.IsStaff()) return Forbid();
        p.Status = ContentStatus.Deleted;
        await db.SaveChangesAsync();
        return NoContent();
    }

    // ----- Commentaires de la publication -----
    [HttpGet("{id:int}/comments")]
    public async Task<IEnumerable<CommentDto>> Comments(int id) =>
        (await db.Comments.Include(c => c.User)
            .Where(c => c.PublicationId == id && c.Status == ContentStatus.Published)
            .OrderBy(c => c.CommentedAt).ToListAsync()).Select(c => c.ToDto());

    /// <summary>AjouterCommentaire / Répondre (ParentId renseigné).</summary>
    [Authorize, HttpPost("{id:int}/comments")]
    public async Task<ActionResult<CommentDto>> AddComment(int id, CommentRequest r)
    {
        var pub = await db.Publications.FindAsync(id);
        if (pub is null || pub.Status != ContentStatus.Published) return NotFound();
        if (r.ParentId.HasValue)
        {
            var parent = await db.Comments.FindAsync(r.ParentId.Value);
            if (parent is null || parent.PublicationId != id || parent.Status != ContentStatus.Published)
                return BadRequest(new { message = "Commentaire parent invalide." });
        }
        var c = new Comment { PublicationId = id, UserId = User.UserId(), Content = r.Content.Trim(), ParentId = r.ParentId };
        db.Comments.Add(c);
        await db.SaveChangesAsync();
        await db.Entry(c).Reference(x => x.User).LoadAsync();
        return c.ToDto();
    }

    // ----- Signaler un contenu -----
    [Authorize, HttpPost("{id:int}/report")]
    public async Task<IActionResult> ReportContent(int id, ReportRequest r)
    {
        if (!await db.Publications.AnyAsync(p => p.Id == id && p.Status == ContentStatus.Published)) return NotFound();
        var uid = User.UserId();
        if (await db.Reports.AnyAsync(x => x.PublicationId == id && x.ReporterId == uid && x.Status == ReportStatus.Open))
            return Conflict(new { message = "Vous avez déjà signalé ce contenu." });
        db.Reports.Add(new Report { PublicationId = id, ReporterId = uid, Reason = r.Reason.Trim() });
        await db.SaveChangesAsync();
        return Ok(new { message = "Signalement envoyé. Merci." });
    }
}

[Authorize, ApiController, Route("api/comments")]
public class CommentsController(AppDbContext db) : ControllerBase
{
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var c = await db.Comments.FindAsync(id);
        if (c is null) return NotFound();
        if (c.UserId != User.UserId() && !User.IsStaff()) return Forbid();
        c.Status = ContentStatus.Deleted;
        await db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Signaler() : un commentaire.</summary>
    [HttpPost("{id:int}/report")]
    public async Task<IActionResult> ReportContent(int id, ReportRequest r)
    {
        if (!await db.Comments.AnyAsync(c => c.Id == id && c.Status == ContentStatus.Published)) return NotFound();
        var uid = User.UserId();
        if (await db.Reports.AnyAsync(x => x.CommentId == id && x.ReporterId == uid && x.Status == ReportStatus.Open))
            return Conflict(new { message = "Vous avez déjà signalé ce contenu." });
        db.Reports.Add(new Report { CommentId = id, ReporterId = uid, Reason = r.Reason.Trim() });
        await db.SaveChangesAsync();
        return Ok(new { message = "Signalement envoyé. Merci." });
    }
}

// ============ Signalements : Examiner() / Résoudre() — modération ============
[Authorize(Roles = "Admin,SuperAdmin"), ApiController, Route("api/reports")]
public class ReportsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IEnumerable<ReportDto>> List(ReportStatus? status)
    {
        var q = db.Reports.Include(r => r.Reporter).Include(r => r.Comment).Include(r => r.Publication).AsQueryable();
        if (status.HasValue) q = q.Where(r => r.Status == status.Value);
        var rows = await q.OrderBy(r => r.Status).ThenByDescending(r => r.ReportedAt).ToListAsync();
        return rows.Select(r => new ReportDto(
            r.Id, r.Reason, r.ReportedAt, r.Status.ToString(), r.Reporter?.FullName ?? "",
            r.CommentId.HasValue ? "Commentaire" : "Publication",
            r.CommentId ?? r.PublicationId ?? 0,
            Shorten(r.Comment?.Content ?? r.Publication?.Title ?? "(contenu supprimé)"),
            r.ModeratorNote));
    }

    [HttpPut("{id:int}/examine")]
    public async Task<IActionResult> Examine(int id)
    {
        var r = await db.Reports.FindAsync(id);
        if (r is null) return NotFound();
        r.Examine();
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut("{id:int}/resolve")]
    public async Task<IActionResult> Resolve(int id, ResolveReportRequest req)
    {
        var r = await db.Reports.Include(x => x.Comment).Include(x => x.Publication).FirstOrDefaultAsync(x => x.Id == id);
        if (r is null) return NotFound();
        if (req.RemoveContent)
        {
            if (r.Comment is not null) r.Comment.Status = ContentStatus.Hidden;
            if (r.Publication is not null) r.Publication.Status = ContentStatus.Hidden;
        }
        r.Resolve(req.RemoveContent, req.Note);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private static string Shorten(string s) => s.Length <= 140 ? s : s[..140] + "…";
}
