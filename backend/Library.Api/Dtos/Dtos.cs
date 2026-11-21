using System.ComponentModel.DataAnnotations;
using Library.Api.Models;

namespace Library.Api.Dtos;

public record RegisterRequest([Required, MinLength(2)] string FullName, [Required, EmailAddress] string Email, [Required, MinLength(6)] string Password);
public record LoginRequest([Required] string Email, [Required] string Password);
public record UpdateProfileRequest([Required, MinLength(2)] string FullName, string? CurrentPassword, string? NewPassword);
public record UserDto(int Id, string FullName, string Email, string Role, DateTime RegisteredAt);
public record AuthResponse(string Token, UserDto User);

public record CategoryRequest([Required] string Name, string? Description);
public record CategoryDto(int Id, string Name, string Description, int BookCount);

public record BookRequest(
    [Required] string Title, [Required] string Author, string? Description, string? Cover, string? File,
    decimal Price, bool IsFree, int CategoryId, BookStatus Status = BookStatus.Published);
public record BookDto(int Id, string Title, string Author, string Description, string Cover, decimal Price,
    bool IsFree, DateTime PublishedAt, string Status, int CategoryId, string CategoryName);
public record BookDetailDto(BookDto Book, bool HasAccess, bool CanBuy);
public record ReadBookDto(int Id, string Title, string Author, string Content);

public record PayRequest(string? CardHolder, string? CardNumber, string? Expiry, string? Cvc, bool Cancelled = false);
public record AuthorizeDto(bool Authorized, string Message, decimal Amount, string Title);
public record PurchaseResultDto(string Status, string Message, string? Reference, int? PurchaseId);
public record PurchaseDto(int Id, int BookId, string BookTitle, string BookCover, DateTime PurchasedAt,
    decimal Amount, string Status, string PaymentReference);

public record PublicationRequest([Required, MinLength(3)] string Title, [Required, MinLength(3)] string Content, string? Type, int? BookId);
public record PublicationDto(int Id, string Title, string Content, string Type, DateTime PublishedAt, string Status,
    int UserId, string Author, int? BookId, string? BookTitle, int CommentCount);

public record CommentRequest([Required, MinLength(1)] string Content, int? ParentId);
public record CommentDto(int Id, string Content, DateTime CommentedAt, string Status, int? ParentId, int PublicationId, int UserId, string Author);

public record ReportRequest([Required, MinLength(3)] string Reason);
public record ReportDto(int Id, string Reason, DateTime ReportedAt, string Status, string Reporter, string TargetType,
    int TargetId, string TargetContent, string? ModeratorNote);
public record ResolveReportRequest(bool RemoveContent, string? Note);

public record StatusRequest(ContentStatus Status);
public record RoleRequest(UserRole Role);
public record CreateAdminRequest([Required] string FullName, [Required, EmailAddress] string Email, [Required, MinLength(6)] string Password);
