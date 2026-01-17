using LiveAuction.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LiveAuction.Infrastructure.Presistence;
internal class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options): IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Auction> Auctions { get; set; }
    public DbSet<Bid> Bids { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<WatchList> WatchLists { get; set; }
    public DbSet<WatchListItem> WatchListItems { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        
    }
}
