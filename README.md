# Live Auction System

A real-time bidding platform built with .NET 10 and Blazor WebAssembly.
The project focuses on handling real-time data synchronization and concurrency challenges in a scalable way.

- **Live Demo:** https://liveauction.runasp.net/
- **Source Code:** https://github.com/fathyali11/LiveAuction

## Architecture
The solution follows **Clean Architecture** principles to ensure separation of concerns.
It implements the **CQRS** pattern using **MediatR** to decouple command and query responsibilities.

## Key Features

### Real-Time Communication
Integrated **SignalR** to broadcast bids and notifications to all connected clients instantly. Users see price updates in real-time without page refreshes.

### Concurrency Control
Implemented robust **Database Transactions** to handle race conditions. This ensures data integrity by preventing simultaneous conflicting bids from different users.

### Automation
Engineered **Background Jobs** to manage the auction lifecycle automatically:
- Closing auctions when time expires.
- Transferring funds from the winner's wallet to the seller.

### Security & UX
Used **HTTP Interceptors** to handle **Refresh Tokens** automatically, keeping the user session active securely without interrupting the experience.

## Tech Stack
* **Framework:** .NET 10, ASP.NET Core Web API
* **Client:** Blazor WebAssembly
* **Real-Time:** SignalR
* **Database:** SQL Server, Entity Framework Core
* **Patterns:** Clean Architecture, CQRS, MediatR
* **Deployment:** MonsterASP

## Author
**Fathy Ali**
[LinkedIn](https://www.linkedin.com/in/fathy-ali-backend/) | [GitHub](https://github.com/fathyali11)
