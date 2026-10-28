# Live Auction System

A real-time bidding platform built with **.NET 10** and **Blazor WebAssembly**.
The project focuses on real-time data synchronization, bid integrity, and concurrency control under load.

- **Live Demo:** https://liveauction.runasp.net/
- **Source Code:** https://github.com/fathyali11/LiveAuction

## Architecture

The solution follows **Clean Architecture** to keep business rules independent of frameworks and infrastructure.
It applies **CQRS** with **MediatR**, so every use case is a small, testable command or query handler.
Expected failures are returned as values (`OneOf<Error, T>`) instead of exceptions, and validation runs through **FluentValidation** before any handler logic.

```
┌──────────────────────┐        HTTP / SignalR        ┌────────────────────┐
│  Blazor WASM Client  │ ───────────────────────────► │  ASP.NET Core API  │
└──────────────────────┘                              └─────────┬──────────┘
                                                                │
                                             ┌──────────────────┴──────────────────┐
                                             ▼                                     ▼
                               LiveAuction.Application              LiveAuction.Infrastructure
                                             │                                     │
                                             └──────────────────┬──────────────────┘
                                                                ▼
                                                       LiveAuction.Domain
```

| Project | Responsibility |
| --- | --- |
| `LiveAuction.Domain` | Entities, enums, repository contracts, and authoritative bidding rules. No dependencies. |
| `LiveAuction.Application` | Commands, queries, validators, service interfaces, mappings, and cross-cutting business logic. |
| `LiveAuction.Infrastructure` | EF Core + SQL Server persistence, repositories, ASP.NET Identity, JWT issuing, Hangfire jobs, and file storage. |
| `LiveAuction.API` | Controllers, SignalR hub, authentication/authorization, rate limiting, and hosting of the Blazor app. |
| `LiveAuction.Client` | Blazor WebAssembly UI (MudBlazor, Refit, SignalR client, LocalStorage). |
| `LiveAuction.Shared` | DTOs and enums shared by both the API and the client, preventing contract drift. |

## Key Features

### Real-time Communication

**SignalR** pushes updates to connected clients through the authenticated hub at `/hubs/auction`.

- Clients join a group per auction, so bid broadcasts only reach interested users.
- Server events: `NewBidPlaced`, `NewNotification`, and `ForceRefreshWallet`.
- Client methods: `JoinToGroup`, `LeaveFromGroup`.

### Bid Integrity & Concurrency Control

Placing a bid is treated as a critical section rather than a simple insert:

- **Atomic conditional update** — a single SQL `UPDATE ... OUTPUT` re-checks the auction status, start/end time, and minimum amount using the database clock, so no two servers can disagree about whether a bid is valid.
- **Per-auction serialization** — `sp_getapplock` (transaction-owned) serializes bids for the same auction and is released automatically when the transaction ends.
- **Idempotent retries** — every bid carries a client-generated idempotency key, enforced by a unique filtered index. A retried request returns the original bid instead of creating a duplicate.
- **Wallet holds instead of immediate charges** — the current highest bidder's funds are held, and the previous bidder's hold is released. Raising your own bid only holds the difference.
- **Anti-sniping protection** — bids in the last 5 minutes extend the auction by 1 minute, up to 30 extensions, and the termination job is rescheduled.
- **Rate limiting** — 10 bid attempts per 10 seconds per user, and 5 auction creations per minute per user.

Bids, wallet ledger entries, and notifications are committed in a **single database transaction**, so a failed bid cannot leave money or notifications behind.

### Automated Auction Lifecycle

**Hangfire** background jobs close auctions when their time expires and then:

- Determine the winner from the highest bid.
- Transfer the held funds from the winner's wallet to the seller.
- Write `Purchase` / `Sale` ledger entries and persist notifications.

### Wallets, Watchlist & Notifications

- Deposit funds, track available vs. locked balance, and browse a paginated transaction history (`Deposit`, `Hold`, `Release`, `Purchase`, `Sale`).
- Add or remove auctions from a watchlist and see the current price live.
- Receive in-app notifications for successful bids, outbids, wallet changes, and auction results, with unread counts and mark-as-read support.

### Security & UX

- **JWT authentication** with short-lived access tokens (60 minutes) and rotating refresh tokens (7 days).
- Refresh-token handling is centralized in an **HTTP delegating handler**, so an expired session is renewed transparently.
- SignalR authenticates WebSocket connections using the JWT passed via the `access_token` query string.
- Image uploads are validated by extension, content type, size (max 5 MB), and file signature before being stored.
- Global exception handling, structured logging with **Serilog**, and a consistent Arabic error experience.
- Fully **RTL Arabic** UI built with MudBlazor, responsive across desktop and mobile.

## Tech Stack

| Area | Technology |
| --- | --- |
| Runtime | .NET 10 |
| Backend | ASP.NET Core Web API |
| Frontend | Blazor WebAssembly + MudBlazor |
| Real-time | SignalR |
| Database | SQL Server + Entity Framework Core |
| Patterns | Clean Architecture, CQRS, MediatR, Repository, Unit of Work |
| Validation & Mapping | FluentValidation, Mapster |
| Background Jobs | Hangfire (SQL Server storage) |
| Auth | ASP.NET Identity, JWT Bearer, Refresh Tokens |
| Client HTTP | Refit |
| Logging | Serilog (console + rolling files) |
| Deployment | MonsterASP (single-origin: the API serves the WASM client) |

## API Overview

| Method | Route | Auth | Description |
| --- | --- | --- | --- |
| `POST` | `/api/auths/register` | – | Create an account and receive tokens. |
| `POST` | `/api/auths/login` | – | Sign in and receive tokens. |
| `POST` | `/api/auths/refresh-token` | – | Rotate an expired access token. |
| `GET` | `/api/auctions` | – | Paginated active auctions (optionally search). |
| `GET` | `/api/auctions/{id}` | – | Auction details with bid history. |
| `POST` | `/api/auctions` | ✔ | Create an auction (multipart form with image). |
| `DELETE` | `/api/auctions/{id}` | ✔ | Delete an auction owned by the caller (no bids yet). |
| `POST` | `/api/bids` | ✔ | Place a bid (accepts `IdempotencyKey`). |
| `GET` | `/api/bids/user-history` | ✔ | The caller's bid history grouped by auction. |
| `GET` | `/api/wallets/summary` | ✔ | Available, locked, and total balance. |
| `POST` | `/api/wallets/deposit` | ✔ | Deposit funds. |
| `GET` | `/api/wallets/transactions` | ✔ | Paginated transaction history. |
| `GET` | `/api/watchlists` | ✔ | Paginated watchlist. |
| `POST` | `/api/watchlists/toggle/{id}` | ✔ | Add/remove an auction from the watchlist. |
| `GET` | `/api/watchlists/count` | Optional | Number of watched auctions (0 for anonymous users). |
| `PUT` | `/api/watchlists/clear` | ✔ | Clear the watchlist. |
| `GET` | `/api/notifications` | ✔ | All notifications for the caller. |
| `GET` | `/api/notifications/unread/count` | ✔ | Unread notification count. |
| `PUT` | `/api/notifications/mark-as-read/{id}` | ✔ | Mark one notification as read. |
| `PUT` | `/api/notifications/mark-all-as-read` | ✔ | Mark every notification as read. |

Swagger is available at `/swagger`, and the Hangfire dashboard at `/jobs`.

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server (LocalDB, Docker, or a full instance)

### Configuration

The API reads its connection string and JWT signing key from configuration. Keep them out of source control using user secrets (recommended for development) or environment variables:

```bash
cd LiveAuction.API

dotnet user-secrets set "ConnectionStrings:LiveAuctionDbConnection" "Server=localhost,1433;Database=LiveAuctionDb;User Id=sa;Password=<your-password>;TrustServerCertificate=True;Encrypt=False;MultipleActiveResultSets=True"
dotnet user-secrets set "JwtSettings:SecretKey" "<a-long-random-secret-at-least-32-chars>"
```

Or with environment variables:

```bash
export ConnectionStrings__LiveAuctionDbConnection="<connection-string>"
export JwtSettings__SecretKey="<secret>"
```

| Setting | Default | Purpose |
| --- | --- | --- |
| `ConnectionStrings:LiveAuctionDbConnection` | – | SQL Server database. |
| `JwtSettings:Issuer` | `LiveAuctionAPI` | JWT issuer. |
| `JwtSettings:Audience` | `LiveAuctionClient` | JWT audience. |
| `JwtSettings:ExpiryInMinutes` | `60` | Access-token lifetime. |
| `JwtSettings:RefreshTokenExpiryInDays` | `7` | Refresh-token lifetime. |
| `JwtSettings:SecretKey` | – | Symmetric signing key (required). |

> Never commit real credentials. If a connection string with a password has been committed, rotate that password and move the value to user secrets or environment variables.

### Run

```bash
dotnet restore
dotnet build

# API (also serves the Blazor client at the root in production)
dotnet run --project LiveAuction.API
```

- App: https://localhost:7293
- Swagger: https://localhost:7293/swagger
- Hangfire dashboard: https://localhost:7293/jobs

EF Core migrations are applied automatically on startup (`Database.MigrateAsync`). To apply them manually:

```bash
dotnet ef database update --project LiveAuction.Infrastructure --startup-project LiveAuction.API
```

### Running the client separately

The Blazor client can also run on its own dev server:

```bash
dotnet run --project LiveAuction.Client
```

This profile runs at https://localhost:7085. The client's API base URL is defined in
`LiveAuction.Client/Consts/HostUrl.cs`; `Current` is hardcoded to `Production` in the repository, so point it at
`Development` (`https://localhost:7293`) when testing against a locally running API. The API CORS policy
already allows `https://localhost:7085`.

## Project Structure

```
LiveAuction.sln
├── LiveAuction.API/            # Controllers, SignalR hub, auth, rate limiting, static hosting
├── LiveAuction.Application/    # CQRS handlers, validators, service contracts, mappings
├── LiveAuction.Client/         # Blazor WASM pages, components, Refit clients, SignalR connections
├── LiveAuction.Domain/         # Entities, enums, repository interfaces, BidRules
├── LiveAuction.Infrastructure/ # EF Core, repositories, Identity/JWT, Hangfire, file storage
└── LiveAuction.Shared/         # DTOs and enums shared by the API and the client
```

## Author

**Fathy Ali**
[LinkedIn](https://www.linkedin.com/in/fathy-ali-backend/) | [GitHub](https://github.com/fathyali11)
