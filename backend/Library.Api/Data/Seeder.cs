using Library.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Library.Api.Data;

public static class Seeder
{
    public static async Task InitAsync(AppDbContext db, ILogger logger)
    {
        for (var i = 0; i < 30; i++)
        {
            try { await db.Database.EnsureCreatedAsync(); break; }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Connexion PostgreSQL impossible, nouvelle tentative ({Attempt}/30)", i + 1);
                if (i < 29)
                    await Task.Delay(2000);
            }
        }

        await BookContents.ApplyAsync(db);

        if (await db.Users.AnyAsync()) return;
        var rnd = new Random(42);
        var now = DateTime.UtcNow;
        string Hash(string p) => BCrypt.Net.BCrypt.HashPassword(p);

        // ---- Utilisateurs ----
        var superAdmin = new User { FullName = "Super Administrateur", Email = "superadmin@biblio.com", PasswordHash = Hash("Super@123"), Role = UserRole.SuperAdmin, RegisteredAt = now.AddDays(-90) };
        var admin = new User { FullName = "Administrateur", Email = "admin@biblio.com", PasswordHash = Hash("Admin@123"), Role = UserRole.Admin, RegisteredAt = now.AddDays(-80) };
        var readers = new[]
        {
            "Lecteur Démo|reader@biblio.com", "Amine Tahiri|amine@demo.com", "Sofia Lamrani|sofia@demo.com",
            "Hamza Ouazzani|hamza@demo.com", "Imane Chraibi|imane@demo.com", "Yassine Berrada|yassine@demo.com"
        }.Select(s =>
        {
            var p = s.Split('|');
            return new User { FullName = p[0], Email = p[1], PasswordHash = Hash("Reader@123"), RegisteredAt = now.AddDays(-rnd.Next(2, 150)) };
        }).ToList();
        db.Users.AddRange(superAdmin, admin);
        db.Users.AddRange(readers);

        // ---- Catégories ----
        var cats = new Dictionary<string, Category>
        {
            ["Roman"] = new() { Name = "Roman", Description = "Fictions et récits contemporains." },
            ["Science"] = new() { Name = "Science", Description = "Comprendre le monde et le vivant." },
            ["Histoire"] = new() { Name = "Histoire", Description = "Civilisations, routes et mémoires." },
            ["Philosophie"] = new() { Name = "Philosophie", Description = "Questions essentielles, réponses ouvertes." },
            ["Technologie"] = new() { Name = "Technologie", Description = "Développement, réseaux et intelligence artificielle." },
            ["Poésie"] = new() { Name = "Poésie", Description = "Vers libres et formes brèves." },
        };
        db.Categories.AddRange(cats.Values);

        // ---- Livres (titres et auteurs fictifs, contenu d'exemple) ----
        Book B(string title, string author, string cat, decimal price, string colors, string desc, int daysAgo) => new()
        {
            Title = title, Author = author, Category = cats[cat], Price = price, IsFree = price == 0,
            Cover = colors, Description = desc, File = SampleText(title, author),
            PublishedAt = now.AddDays(-daysAgo)
        };
        var books = new List<Book>
        {
            B("Les Heures Bleues", "Salma Berrada", "Roman", 59, "#0a84ff,#5e5ce6", "Une traductrice rentre à Tanger et rouvre la maison de son enfance, le temps d'un été.", 70),
            B("Le Jardin de Fès", "Youssef Amrani", "Roman", 0, "#30d158,#0a84ff", "Trois générations se partagent un riad, un figuier et un secret de famille.", 65),
            B("Nuit sur l'Atlas", "Leïla Benjelloun", "Roman", 79, "#bf5af2,#ff375f", "Un convoi, une tempête de neige et six voyageurs qui n'auraient jamais dû se croiser.", 40),
            B("Cosmos pour tous", "Karim Idrissi", "Science", 89, "#5e5ce6,#000000", "Des étoiles aux trous noirs : l'astrophysique expliquée sans équation.", 55),
            B("L'ADN raconté", "Nadia Cherkaoui", "Science", 0, "#64d2ff,#30d158", "Comment une molécule de deux nanomètres écrit l'histoire du vivant.", 33),
            B("Routes de la soie", "Omar Tazi", "Histoire", 69, "#ff9f0a,#ff375f", "Marchands, caravanes et idées : mille ans d'échanges entre l'Orient et l'Occident.", 48),
            B("Al-Andalus, mémoire vive", "Hind El Fassi", "Histoire", 99, "#ffd60a,#ff9f0a", "Une histoire de convivance, de savoirs partagés et de transmission.", 25),
            B("Penser le temps", "Amine Lahlou", "Philosophie", 49, "#8e8e93,#2c2c2e", "Mémoire, attente, urgence : ce que le temps fait à nos vies.", 60),
            B("Petit traité du doute", "Sara Mernissi", "Philosophie", 0, "#ac8e68,#2c2c2e", "Douter pour mieux décider : un manuel de lucidité en dix chapitres.", 20),
            B("Concevoir une API durable", "Mehdi Alaoui", "Technologie", 129, "#0a84ff,#64d2ff", "Versionnement, sécurité, observabilité : les pratiques qui tiennent dans le temps.", 15),
            B("Réseaux sans mystère", "Ilham Ziani", "Technologie", 109, "#30d158,#003a1b", "Du câble au cloud : modèle OSI, TCP/IP, DNS et supervision, pas à pas.", 12),
            B("Introduction à l'IA", "Reda Benkirane", "Technologie", 0, "#ff375f,#5e5ce6", "Apprentissage automatique, réseaux de neurones et limites des modèles.", 8),
            B("Sable et lumière", "Zineb Haddad", "Poésie", 39, "#ffd60a,#bf5af2", "Quarante poèmes brefs entre dune, mer et ville blanche.", 4),
        };
        db.Books.AddRange(books);
        await db.SaveChangesAsync();

        // ---- Achats d'exemple sur 30 jours (alimentent les graphiques) ----
        var paid = books.Where(b => !b.IsFree).ToList();
        for (var i = 0; i < 60; i++)
        {
            var u = readers[rnd.Next(readers.Count)];
            var b = paid[rnd.Next(paid.Count)];
            var when = now.AddDays(-rnd.Next(0, 30)).AddHours(-rnd.Next(0, 24));
            var roll = rnd.Next(100);
            var status = roll < 80 ? PurchaseStatus.Success : roll < 92 ? PurchaseStatus.Failed : PurchaseStatus.Cancelled;
            if (status == PurchaseStatus.Success && db.Purchases.Local.Any(p => p.UserId == u.Id && p.BookId == b.Id && p.Status == PurchaseStatus.Success)) continue;
            var reference = "PAY-" + Guid.NewGuid().ToString("N")[..10].ToUpperInvariant();
            var purchase = new Purchase { UserId = u.Id, BookId = b.Id, Amount = b.Price, PurchasedAt = when };
            var payStatus = status switch { PurchaseStatus.Success => PaymentStatus.Succeeded, PurchaseStatus.Failed => PaymentStatus.Refused, _ => PaymentStatus.Cancelled };
            if (status == PurchaseStatus.Success) purchase.Confirm(reference);
            else if (status == PurchaseStatus.Failed) purchase.Fail(reference);
            else purchase.Cancel(reference);
            purchase.Payment = new Payment { Amount = b.Price, Status = payStatus, Reference = reference, PaidAt = when };
            db.Purchases.Add(purchase);
        }

        // ---- Publications, commentaires, signalement ----
        var p1 = new Publication { Title = "Ce que la nuit change à la lecture", Type = "Reflexion", UserId = readers[1].Id, Content = "Je lis presque exclusivement après minuit. Les phrases semblent plus lentes, les silences plus lourds. Est-ce le livre qui change, ou le lecteur ?", PublishedAt = now.AddDays(-6) };
        var p2 = new Publication { Title = "Mon avis sur « Les Heures Bleues »", Type = "Avis", UserId = readers[2].Id, BookId = books[0].Id, Content = "Un roman discret, tout en retenue. Les chapitres sur la maison de Tanger valent à eux seuls la lecture. Un peu long au milieu, mais la fin récompense.", PublishedAt = now.AddDays(-3) };
        var p3 = new Publication { Title = "Pourquoi relire un livre gratuit ?", Type = "Reflexion", UserId = readers[3].Id, Content = "Les livres gratuits sont souvent ceux que l'on ouvre sans attente. C'est peut-être pour cela qu'on s'en souvient mieux.", PublishedAt = now.AddDays(-1) };
        db.Publications.AddRange(p1, p2, p3);
        await db.SaveChangesAsync();

        var c1 = new Comment { PublicationId = p2.Id, UserId = readers[1].Id, Content = "Entièrement d'accord sur les chapitres de Tanger, magnifiques.", CommentedAt = now.AddDays(-2) };
        var c2 = new Comment { PublicationId = p1.Id, UserId = readers[4].Id, Content = "Pareil pour moi : la nuit, je lis trois fois plus lentement et je retiens chaque phrase.", CommentedAt = now.AddDays(-5) };
        db.Comments.AddRange(c1, c2);
        await db.SaveChangesAsync();

        var reply = new Comment { PublicationId = p2.Id, ParentId = c1.Id, UserId = readers[2].Id, Content = "Merci ! J'ai adoré l'ambiance de ce quartier.", CommentedAt = now.AddDays(-1) };
        db.Comments.Add(reply);

        db.Reports.Add(new Report
        {
            Reason = "Propos hors-sujet à vérifier par la modération.",
            ReporterId = readers[1].Id,
            CommentId = c2.Id,
            Status = ReportStatus.Open,
            ReportedAt = now.AddHours(-12)
        });
        await db.SaveChangesAsync();
    }

    private static string SampleText(string title, string author) => $"{title}\n{author}\n\nChapitre 1 — Ouverture\nContenu initial.";
}
