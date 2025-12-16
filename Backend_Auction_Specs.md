# Task: Implement CRUD Operations for Auction Entity
**Role:** Senior .NET Backend Developer
**Architecture:** Clean Architecture with CQRS (MediatR) & Repository Pattern.
**Library:** Mapster (for mapping), EF Core.
**Base Namespace:** `LiveAuction.Application.Auctions` (Note: We are NOT using a "Features" folder).

## Context
We need to implement the remaining CRUD operations for the `Auction` entity.
The `Create` and `GetAll` are already implemented.
We need to add: **GetById**, **Update**, and **Delete**.

## Requirements

### 1. Query: Get Auction By Id
- **Namespace/Path:** `LiveAuction.Application.Auctions.Queries.GetAuctionById`
- **Request:** `GetAuctionByIdQuery(int Id)` -> Returns `AuctionDto?`
- **Logic:** - Use `IAuctionRepository.GetByIdAsync(id)`. 
    - Use Mapster (`.Adapt<AuctionDto>()`) to map the entity.

### 2. Command: Update Auction
- **Namespace/Path:** `LiveAuction.Application.Auctions.Commands.UpdateAuction`
- **Request:** `UpdateAuctionCommand` (Properties: Id, ItemName, Description, ImageUrl). **Note:** Do NOT include Price or EndTime.
- **Business Rules (CRITICAL):**
    1. Check if the auction exists.
    2. Check if the auction has any existing Bids (use `_repo.GetByIdWithBidsAsync`).
    3. **Rule:** If `auction.Bids.Count > 0`, throw an exception (or return false), because modifying an active auction with bids is forbidden.
    4. If no bids, update `ItemName`, `Description`, and `ImageUrl`.
    5. Save changes using `_repo.UpdateAsync`.

### 3. Command: Delete Auction
- **Namespace/Path:** `LiveAuction.Application.Auctions.Commands.DeleteAuction`
- **Request:** `DeleteAuctionCommand(int Id)` -> Returns `bool`
- **Business Rules (CRITICAL):**
    1. Check if the auction exists.
    2. Check if the auction has any existing Bids.
    3. **Rule:** If `auction.Bids.Any()`, throw an exception. We cannot delete an auction that has collected money/bids.
    4. If no bids, proceed with `_repo.DeleteAsync`.

### 4. API Controller Updates
- **Path:** `API/Controllers/AuctionsController.cs`
- Add endpoints for the above handlers:
    - `GET api/auctions/{id}`
    - `PUT api/auctions/{id}` (Requires `[Authorize]`)
    - `DELETE api/auctions/{id}` (Requires `[Authorize]`, possibly Admin role or Owner check)

## Repository Reference
Assume `IAuctionRepository` has:
- `GetByIdAsync(int id)`
- `GetByIdWithBidsAsync(int id)` (Critical for validation)
- `UpdateAsync(Auction auction)`
- `DeleteAsync(int id)`