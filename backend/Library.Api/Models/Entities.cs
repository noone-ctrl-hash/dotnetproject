namespace Library.Api.Models;

public enum UserRole { Reader, Admin, SuperAdmin }
public enum BookStatus { Published, Hidden }
public enum PurchaseStatus { Pending, Success, Failed, Cancelled }
public enum PaymentStatus { Pending, Succeeded, Refused, Cancelled }
public enum ContentStatus { Published, Hidden, Deleted }
public enum ReportStatus { Open, UnderReview, Resolved, Rejected }

public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public UserRole Role { get; set; } = UserRole.Reader;
    public List<Purchase> Purchases { get; set; } = new();
}

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public List<Book> Books { get; set; } = new();
}

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Author { get; set; } = "";
    public string Description { get; set; } = "";
    /// <summary>URL d'une image, ou deux couleurs "#hex,#hex" pour générer une couverture.</summary>
    public string Cover { get; set; } = "";
    /// <summary>Contenu du livre (texte).</summary>
    public string File { get; set; } = "";
    public decimal Price { get; set; }
    public bool IsFree { get; set; }
    public DateTime PublishedAt { get; set; } = DateTime.UtcNow;
    public BookStatus Status { get; set; } = BookStatus.Published;
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
}

public class Purchase
{
    public int Id { get; set; }
    public DateTime PurchasedAt { get; set; } = DateTime.UtcNow;
    public decimal Amount { get; set; }
    public PurchaseStatus Status { get; set; } = PurchaseStatus.Pending;
    public string PaymentReference { get; set; } = "";
    public int UserId { get; set; }
    public User? User { get; set; }
    public int BookId { get; set; }
    public Book? Book { get; set; }
    public Payment? Payment { get; set; }

    public void Confirm(string reference) { Status = PurchaseStatus.Success; PaymentReference = reference; }
    public void Fail(string reference) { Status = PurchaseStatus.Failed; PaymentReference = reference; }
    public void Cancel(string reference) { Status = PurchaseStatus.Cancelled; PaymentReference = reference; }
}

public class Payment
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public DateTime PaidAt { get; set; } = DateTime.UtcNow;
    public string Reference { get; set; } = "";
    public int PurchaseId { get; set; }
    public Purchase? Purchase { get; set; }
}

public class Publication
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Content { get; set; } = "";
    /// <summary>"Reflexion" ou "Avis".</summary>
    public string Type { get; set; } = "Reflexion";
    public DateTime PublishedAt { get; set; } = DateTime.UtcNow;
    public ContentStatus Status { get; set; } = ContentStatus.Published;
    public int UserId { get; set; }
    public User? User { get; set; }
    public int? BookId { get; set; }
    public Book? Book { get; set; }
    public List<Comment> Comments { get; set; } = new();
}

public class Comment
{
    public int Id { get; set; }
    public string Content { get; set; } = "";
    public DateTime CommentedAt { get; set; } = DateTime.UtcNow;
    public ContentStatus Status { get; set; } = ContentStatus.Published;
    public int? ParentId { get; set; }
    public Comment? Parent { get; set; }
    public List<Comment> Replies { get; set; } = new();
    public int PublicationId { get; set; }
    public Publication? Publication { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
}

public class Report
{
    public int Id { get; set; }
    public string Reason { get; set; } = "";
    public DateTime ReportedAt { get; set; } = DateTime.UtcNow;
    public ReportStatus Status { get; set; } = ReportStatus.Open;
    public string? ModeratorNote { get; set; }
    public int ReporterId { get; set; }
    public User? Reporter { get; set; }
    public int? CommentId { get; set; }
    public Comment? Comment { get; set; }
    public int? PublicationId { get; set; }
    public Publication? Publication { get; set; }

    public void Examine() => Status = ReportStatus.UnderReview;
    public void Resolve(bool contentRemoved, string? note)
    {
        Status = contentRemoved ? ReportStatus.Resolved : ReportStatus.Rejected;
        ModeratorNote = note;
    }
}

