# Bibliothèque Numérique (sans Docker)

Bibliothèque en ligne full-stack, conçue à partir des diagrammes de classes, de cas d'utilisation et de séquence.

| Couche | Technologie |
|---|---|
| Front | React 18 + Vite, React Router, Chart.js (react-chartjs-2), thème noir, typographie système Apple (SF Pro) avec Inter en repli |
| API | ASP.NET Core 8 Web API, EF Core, JWT, BCrypt, Swagger |
| Base | PostgreSQL 14 ou plus récent |

## Prérequis

- [.NET SDK 8](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org)
- [PostgreSQL](https://www.postgresql.org/download/) en cours d'exécution sur le port 5432

## Installation

**1. Créer la base**

```bash
psql -U postgres -f database/setup.sql
```

Cela crée l'utilisateur `biblio` (mot de passe `biblio_pwd`) et la base `bibliotheque`.
Si vous utilisez d'autres identifiants, modifiez `ConnectionStrings:Default` dans `backend/Library.Api/appsettings.json`.

**2. Lancer l'API** (terminal 1)

```bash
cd backend/Library.Api
dotnet run
```

API sur http://localhost:5000, documentation sur http://localhost:5000/swagger.
Au premier lancement, les tables sont créées et des données de démonstration sont insérées (13 livres, achats sur 30 jours, publications, commentaires, un signalement).

**3. Lancer le frontend** (terminal 2)

```bash
cd frontend
npm install
npm run dev
```

Application sur http://localhost:5173 (les appels `/api` sont redirigés vers `localhost:5000`).

Raccourcis : `run-api.sh` / `run-web.sh` (macOS, Linux) ou `run-api.bat` / `run-web.bat` (Windows).

## Comptes de démonstration

| Rôle | E-mail | Mot de passe |
|---|---|---|
| Lecteur | reader@biblio.com | Reader@123 |
| Administrateur | admin@biblio.com | Admin@123 |
| Super administrateur | superadmin@biblio.com | Super@123 |

## Couverture des diagrammes

**Classes** : Utilisateur, Catégorie, Livre, Achat, Paiement, Publication, Commentaire (réponses via `ParentId`), Signalement,
avec leurs méthodes métier (`Achat.Confirm/Cancel`, `Signalement.Examine/Resolve`, etc.).

**Cas d'utilisation**

| Acteur | Fonctionnalités | Où |
|---|---|---|
| Visiteur | Créer un compte, consulter le catalogue et les détails | `/inscription`, `/`, `/livres/:id` |
| Lecteur | Se connecter, rechercher, acheter (paiement simulé), lire un livre gratuit, accéder aux livres achetés, historique des achats, publier une réflexion ou un avis, commenter, répondre, signaler | `/bibliotheque`, `/publications`, `/lire/:id` |
| Administrateur | Gérer catégories et livres, modérer contenus et signalements, consulter les statistiques (Chart.js) | `/admin` |
| Super administrateur | Gérer rôles et permissions, utilisateurs, administrateurs | `/super-admin` |

**Séquence « Achat et accès à un livre »**

1. `POST /api/purchases/{id}/authorize` : vérifie utilisateur, livre, statut gratuit/payant, absence d'achat réussi.
2. `POST /api/purchases/{id}/pay` : paiement simulé, puis enregistrement de l'achat en `Success`, `Failed` ou `Cancelled`.
3. `GET /api/books/{id}/read` : vérifie le droit d'accès et renvoie le contenu.

Paiement simulé : toute carte à 16 chiffres est acceptée, `4000 0000 0000 0002` est refusée, « Annuler le paiement » produit un achat annulé.
Pour brancher Stripe, implémentez `IPaymentGateway` (`Services/Services.cs`).

## Mise en production (sans Docker)

```bash
cd frontend && npm run build          # génère frontend/dist
cd backend/Library.Api && dotnet publish -c Release -o publish
```

Servez `frontend/dist` avec un serveur web (Nginx, IIS…) qui redirige `/api` vers l'API, et définissez `Jwt__Key` (32 caractères minimum) et `ConnectionStrings__Default` en variables d'environnement.

## Notes

- Le schéma est créé par `EnsureCreated`. Pour des migrations EF Core : `dotnet ef migrations add Init`, puis remplacez `EnsureCreatedAsync` par `MigrateAsync` dans `Data/Seeder.cs`.
- Le rôle est lu dans le jeton JWT : un changement de rôle s'applique à la prochaine connexion.
- Titres, auteurs et textes des livres de démonstration sont fictifs.
