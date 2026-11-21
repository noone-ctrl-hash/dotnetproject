using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Library.Api.Data;
using Library.Api.Dtos;
using Library.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Library.Api.Services;

public static class ClaimsExtensions
{
    public static int UserId(this ClaimsPrincipal p) => int.Parse(p.FindFirstValue(ClaimTypes.NameIdentifier)!);
    public static bool IsStaff(this ClaimsPrincipal p) => p.IsInRole("Admin") || p.IsInRole("SuperAdmin");
}

public static class Map
{
    public static UserDto ToDto(this User u) => new(u.Id, u.FullName, u.Email, u.Role.ToString(), u.RegisteredAt);

    public static BookDto ToDto(this Book b) => new(b.Id, b.Title, b.Author, b.Description, b.Cover, b.Price,
        b.IsFree, b.PublishedAt, b.Status.ToString(), b.CategoryId, b.Category?.Name ?? "");

    public static PublicationDto ToDto(this Publication p, int commentCount) => new(p.Id, p.Title, p.Content, p.Type,
        p.PublishedAt, p.Status.ToString(), p.UserId, p.User?.FullName ?? "", p.BookId, p.Book?.Title, commentCount);

    public static CommentDto ToDto(this Comment c) => new(c.Id, c.Content, c.CommentedAt, c.Status.ToString(),
        c.ParentId, c.PublicationId, c.UserId, c.User?.FullName ?? "");
}

public class TokenService(IConfiguration cfg)
{
    public string Create(User u)
    {
        var jwtKey = cfg["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is not configured.");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, u.Id.ToString()),
            new Claim(ClaimTypes.Email, u.Email),
            new Claim(ClaimTypes.Name, u.FullName),
            new Claim(ClaimTypes.Role, u.Role.ToString())
        };
        var token = new JwtSecurityToken(
            cfg["Jwt:Issuer"], cfg["Jwt:Audience"], claims,
            expires: DateTime.UtcNow.AddHours(double.Parse(cfg["Jwt:ExpiresHours"] ?? "12")),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

// ---------- Paiement (simulé — remplaçable par Stripe en implémentant IPaymentGateway) ----------
public record PaymentResult(PaymentStatus Status, string Reference, string Message);

public interface IPaymentGateway
{
    Task<PaymentResult> ChargeAsync(decimal amount, PayRequest card);
}

/// <summary>
/// Passerelle de paiement simulée.
/// Carte 4000 0000 0000 0002 = refusée. Toute autre carte à 16 chiffres = acceptée.
/// </summary>
public class SimulatedPaymentGateway : IPaymentGateway
{
    public async Task<PaymentResult> ChargeAsync(decimal amount, PayRequest card)
    {
        await Task.Delay(700);
        var reference = "PAY-" + Guid.NewGuid().ToString("N")[..10].ToUpperInvariant();
        if (card.Cancelled) return new(PaymentStatus.Cancelled, reference, "Paiement annulé.");

        var number = new string((card.CardNumber ?? "").Where(char.IsDigit).ToArray());
        if (number.Length != 16 || string.IsNullOrWhiteSpace(card.Cvc) || string.IsNullOrWhiteSpace(card.Expiry))
            return new(PaymentStatus.Refused, reference, "Informations de carte invalides.");
        if (number == "4000000000000002")
            return new(PaymentStatus.Refused, reference, "Carte refusée par la banque.");
        return new(PaymentStatus.Succeeded, reference, "Paiement accepté.");
    }
}

// ---------- Achat (diagramme de séquence : Purchase / ProcessPayment / ReadBook) ----------
public record AuthResult(bool Ok, string Message, Book? Book);

public class PurchaseService(AppDbContext db, IPaymentGateway gateway)
{
    /// <summary>Vérifier l'utilisateur et le livre, son statut gratuit/payant et l'absence d'achat réussi.</summary>
    public async Task<AuthResult> AuthorizeAsync(int userId, int bookId)
    {
        if (!await db.Users.AnyAsync(u => u.Id == userId)) return new(false, "Utilisateur introuvable.", null);
        var book = await db.Books.FirstOrDefaultAsync(b => b.Id == bookId && b.Status == BookStatus.Published);
        if (book is null) return new(false, "Livre introuvable.", null);
        if (book.IsFree || book.Price <= 0) return new(false, "Ce livre est gratuit : aucun achat nécessaire.", book);
        if (await HasPurchasedAsync(userId, bookId)) return new(false, "Vous possédez déjà ce livre.", book);
        return new(true, "Achat autorisé.", book);
    }

    public Task<bool> HasPurchasedAsync(int userId, int bookId) =>
        db.Purchases.AnyAsync(p => p.UserId == userId && p.BookId == bookId && p.Status == PurchaseStatus.Success);

    /// <summary>Vérifier le droit d'accès (livre gratuit ou achat réussi).</summary>
    public async Task<bool> HasAccessAsync(int userId, Book book) =>
        book.IsFree || book.Price <= 0 || await HasPurchasedAsync(userId, book.Id);

    /// <summary>Simuler le paiement puis enregistrer l'achat (Success / Failed / Cancelled).</summary>
    public async Task<PurchaseResultDto> PayAsync(int userId, int bookId, PayRequest req)
    {
        var auth = await AuthorizeAsync(userId, bookId);
        if (!auth.Ok) return new("Rejected", auth.Message, null, null);
        var book = auth.Book!;

        var result = req.Cancelled
            ? new PaymentResult(PaymentStatus.Cancelled, "PAY-" + Guid.NewGuid().ToString("N")[..10].ToUpperInvariant(), "Paiement annulé.")
            : await gateway.ChargeAsync(book.Price, req);

        var purchase = new Purchase { UserId = userId, BookId = bookId, Amount = book.Price, PurchasedAt = DateTime.UtcNow };
        switch (result.Status)
        {
            case PaymentStatus.Succeeded: purchase.Confirm(result.Reference); break;
            case PaymentStatus.Refused: purchase.Fail(result.Reference); break;
            default: purchase.Cancel(result.Reference); break;
        }
        purchase.Payment = new Payment
        {
            Amount = book.Price, Status = result.Status, Reference = result.Reference, PaidAt = DateTime.UtcNow
        };
        db.Purchases.Add(purchase);
        await db.SaveChangesAsync();

        var message = result.Status switch
        {
            PaymentStatus.Succeeded => "Livre acheté avec succès.",
            PaymentStatus.Refused => result.Message,
            _ => "Achat annulé."
        };
        return new(purchase.Status.ToString(), message, result.Reference, purchase.Id);
    }
}
