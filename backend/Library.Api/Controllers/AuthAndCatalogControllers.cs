using Library.Api.Data;
using Library.Api.Dtos;
using Library.Api.Models;
using Library.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Library.Api.Controllers;

// ============ Auth : créer un compte, se connecter, modifier le profil ============
[ApiController, Route("api/auth")]
public class AuthController(AppDbContext db, TokenService tokens) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest r)
    {
        var email = r.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(u => u.Email == email))
            return Conflict(new { message = "Un compte existe déjà avec cet e-mail." });
        var user = new User
        {
            FullName = r.FullName.Trim(), Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(r.Password), Role = UserRole.Reader
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return new AuthResponse(tokens.Create(user), user.ToDto());
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest r)
    {
        var email = r.Email.Trim().ToLowerInvariant();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user is null || !BCrypt.Net.BCrypt.Verify(r.Password, user.PasswordHash))
            return Unauthorized(new { message = "E-mail ou mot de passe incorrect." });
        return new AuthResponse(tokens.Create(user), user.ToDto());
    }

    [Authorize, HttpGet("me")]
    public async Task<ActionResult<UserDto>> Me()
    {
        var user = await db.Users.FindAsync(User.UserId());
        return user is null ? Unauthorized() : user.ToDto();
    }

    [Authorize, HttpPut("profile")]
    public async Task<ActionResult<AuthResponse>> UpdateProfile(UpdateProfileRequest r)
    {
        var user = await db.Users.FindAsync(User.UserId());
        if (user is null) return Unauthorized();
        user.FullName = r.FullName.Trim();
        if (!string.IsNullOrEmpty(r.NewPassword))
        {
            if (r.NewPassword.Length < 6) return BadRequest(new { message = "Le nouveau mot de passe doit contenir au moins 6 caractères." });
            if (string.IsNullOrEmpty(r.CurrentPassword) || !BCrypt.Net.BCrypt.Verify(r.CurrentPassword, user.PasswordHash))
                return BadRequest(new { message = "Mot de passe actuel incorrect." });
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(r.NewPassword);
        }
        await db.SaveChangesAsync();
        return new AuthResponse(tokens.Create(user), user.ToDto());
    }
}

// ============ Catégories : consultation publique, gestion par l'admin ============
[ApiController, Route("api/categories")]
public class CategoriesController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IEnumerable<CategoryDto>> List() =>
        await db.Categories.OrderBy(c => c.Name)
            .Select(c => new CategoryDto(c.Id, c.Name, c.Description, c.Books.Count(b => b.Status == BookStatus.Published)))
            .ToListAsync();

    [Authorize(Roles = "Admin,SuperAdmin"), HttpPost]
    public async Task<ActionResult<CategoryDto>> Add(CategoryRequest r)
    {
        var name = r.Name.Trim();
        if (await db.Categories.AnyAsync(c => c.Name == name)) return Conflict(new { message = "Cette catégorie existe déjà." });
        var c = new Category { Name = name, Description = r.Description?.Trim() ?? "" };
        db.Categories.Add(c);
        await db.SaveChangesAsync();
        return new CategoryDto(c.Id, c.Name, c.Description, 0);
    }

    [Authorize(Roles = "Admin,SuperAdmin"), HttpPut("{id:int}")]
    public async Task<ActionResult<CategoryDto>> Update(int id, CategoryRequest r)
    {
        var c = await db.Categories.FindAsync(id);
        if (c is null) return NotFound();
        var name = r.Name.Trim();
        if (await db.Categories.AnyAsync(x => x.Name == name && x.Id != id)) return Conflict(new { message = "Ce nom est déjà utilisé." });
        c.Name = name; c.Description = r.Description?.Trim() ?? "";
        await db.SaveChangesAsync();
        return new CategoryDto(c.Id, c.Name, c.Description, await db.Books.CountAsync(b => b.CategoryId == id && b.Status == BookStatus.Published));
    }

    [Authorize(Roles = "Admin,SuperAdmin"), HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var c = await db.Categories.FindAsync(id);
        if (c is null) return NotFound();
        if (await db.Books.AnyAsync(b => b.CategoryId == id))
            return Conflict(new { message = "Impossible de supprimer : des livres appartiennent à cette catégorie." });
        db.Categories.Remove(c);
        await db.SaveChangesAsync();
        return NoContent();
    }
}

// ============ Livres : catalogue, détails, lecture, gestion ============
[ApiController, Route("api/books")]
public class BooksController(AppDbContext db, PurchaseService purchases) : ControllerBase
{
    [HttpGet]
    public async Task<IEnumerable<BookDto>> List(string? search, int? categoryId, bool? free, string? sort)
    {
        var q = db.Books.Include(b => b.Category).Where(b => b.Status == BookStatus.Published);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            q = q.Where(b => b.Title.ToLower().Contains(s) || b.Author.ToLower().Contains(s) || b.Description.ToLower().Contains(s));
        }
        if (categoryId.HasValue) q = q.Where(b => b.CategoryId == categoryId.Value);
        if (free.HasValue) q = q.Where(b => b.IsFree == free.Value);
        var ordered = sort switch
        {
            "price" => q.OrderBy(b => b.Price),
            "title" => q.OrderBy(b => b.Title),
            _ => q.OrderByDescending(b => b.PublishedAt)
        };
        return (await ordered.ToListAsync()).Select(b => b.ToDto());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookDetailDto>> Get(int id)
    {
        var book = await db.Books.Include(b => b.Category).FirstOrDefaultAsync(b => b.Id == id);
        if (book is null || (book.Status != BookStatus.Published && !User.IsStaff())) return NotFound();

        var free = book.IsFree || book.Price <= 0;
        var hasAccess = false;
        var canBuy = !free;
        if (User.Identity?.IsAuthenticated == true)
        {
            hasAccess = User.IsStaff() || await purchases.HasAccessAsync(User.UserId(), book);
            canBuy = !hasAccess;
        }
        return new BookDetailDto(book.ToDto(), hasAccess, canBuy);
    }

    /// <summary>ReadBook : vérifie le droit d'accès puis retourne le contenu.</summary>
    [Authorize, HttpGet("{id:int}/read")]
    public async Task<ActionResult<ReadBookDto>> Read(int id)
    {
        var book = await db.Books.FindAsync(id);
        if (book is null) return NotFound();
        if (!User.IsStaff() && !await purchases.HasAccessAsync(User.UserId(), book))
            return StatusCode(403, new { message = "Achetez ce livre pour y accéder." });
        return new ReadBookDto(book.Id, book.Title, book.Author, book.File);
    }

    /// <summary>Accéder aux livres achetés.</summary>
    [Authorize, HttpGet("library")]
    public async Task<IEnumerable<BookDto>> Library()
    {
        var uid = User.UserId();
        var ids = await db.Purchases
            .Where(p => p.UserId == uid && p.Status == PurchaseStatus.Success)
            .Select(p => p.BookId).Distinct().ToListAsync();
        var books = await db.Books.Include(b => b.Category).Where(b => ids.Contains(b.Id)).ToListAsync();
        return books.Select(b => b.ToDto());
    }

    // ----- Gestion (Admin) -----
    [Authorize(Roles = "Admin,SuperAdmin"), HttpGet("admin")]
    public async Task<IEnumerable<BookDto>> AdminList() =>
        (await db.Books.Include(b => b.Category).OrderByDescending(b => b.PublishedAt).ToListAsync()).Select(b => b.ToDto());

    [Authorize(Roles = "Admin,SuperAdmin"), HttpGet("{id:int}/edit")]
    public async Task<ActionResult<BookRequest>> ForEdit(int id)
    {
        var b = await db.Books.FindAsync(id);
        return b is null ? NotFound() : new BookRequest(b.Title, b.Author, b.Description, b.Cover, b.File, b.Price, b.IsFree, b.CategoryId, b.Status);
    }

    [Authorize(Roles = "Admin,SuperAdmin"), HttpPost]
    public async Task<ActionResult<BookDto>> Add(BookRequest r)
    {
        if (!await db.Categories.AnyAsync(c => c.Id == r.CategoryId)) return BadRequest(new { message = "Catégorie invalide." });
        var b = new Book { PublishedAt = DateTime.UtcNow };
        Apply(b, r);
        db.Books.Add(b);
        await db.SaveChangesAsync();
        await db.Entry(b).Reference(x => x.Category).LoadAsync();
        return b.ToDto();
    }

    [Authorize(Roles = "Admin,SuperAdmin"), HttpPut("{id:int}")]
    public async Task<ActionResult<BookDto>> Update(int id, BookRequest r)
    {
        var b = await db.Books.FindAsync(id);
        if (b is null) return NotFound();
        if (!await db.Categories.AnyAsync(c => c.Id == r.CategoryId)) return BadRequest(new { message = "Catégorie invalide." });
        Apply(b, r);
        await db.SaveChangesAsync();
        await db.Entry(b).Reference(x => x.Category).LoadAsync();
        return b.ToDto();
    }

    /// <summary>Supprime le livre, ou le masque s'il a déjà été acheté.</summary>
    [Authorize(Roles = "Admin,SuperAdmin"), HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var b = await db.Books.FindAsync(id);
        if (b is null) return NotFound();
        if (await db.Purchases.AnyAsync(p => p.BookId == id)) b.Status = BookStatus.Hidden;
        else db.Books.Remove(b);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private static void Apply(Book b, BookRequest r)
    {
        b.Title = r.Title.Trim();
        b.Author = r.Author.Trim();
        b.Description = r.Description?.Trim() ?? "";
        b.Cover = string.IsNullOrWhiteSpace(r.Cover) ? "#0a84ff,#5e5ce6" : r.Cover.Trim();
        b.File = r.File ?? b.File;
        b.IsFree = r.IsFree || r.Price <= 0;
        b.Price = b.IsFree ? 0 : r.Price;
        b.CategoryId = r.CategoryId;
        b.Status = r.Status;
    }
}

// ============ Achats : Purchase → ProcessPayment → historique ============
[Authorize, ApiController, Route("api/purchases")]
public class PurchasesController(AppDbContext db, PurchaseService service) : ControllerBase
{
    /// <summary>Purchase(bookId) : vérifie l'utilisateur, le livre, son statut payant et l'absence d'achat existant.</summary>
    [HttpPost("{bookId:int}/authorize")]
    public async Task<ActionResult<AuthorizeDto>> CheckAuthorization(int bookId)
    {
        var r = await service.AuthorizeAsync(User.UserId(), bookId);
        return new AuthorizeDto(r.Ok, r.Message, r.Book?.Price ?? 0, r.Book?.Title ?? "");
    }

    /// <summary>ProcessPayment(bookId) : paiement simulé puis enregistrement (Success / Failed / Cancelled).</summary>
    [HttpPost("{bookId:int}/pay")]
    public async Task<PurchaseResultDto> Pay(int bookId, PayRequest req) =>
        await service.PayAsync(User.UserId(), bookId, req);

    /// <summary>ConsulterAchats : historique des achats de l'utilisateur.</summary>
    [HttpGet("mine")]
    public async Task<IEnumerable<PurchaseDto>> Mine()
    {
        var uid = User.UserId();
        var rows = await db.Purchases.Include(p => p.Book).Where(p => p.UserId == uid)
            .OrderByDescending(p => p.PurchasedAt).ToListAsync();
        return rows.Select(p => new PurchaseDto(p.Id, p.BookId, p.Book?.Title ?? "", p.Book?.Cover ?? "",
            p.PurchasedAt, p.Amount, p.Status.ToString(), p.PaymentReference));
    }
}
