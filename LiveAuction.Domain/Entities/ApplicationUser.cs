using Microsoft.AspNetCore.Identity;

namespace LiveAuction.Domain.Entities;

public class ApplicationUser:IdentityUser
{
    public string FullName { get; set; }=string.Empty;
    public decimal TotalBalance { get; set; }
    public decimal LockedBalance { get; set; }
    public decimal AvailableBalance => TotalBalance - LockedBalance;
    public WatchList WatchList { get; set; } = null!;
    public ICollection<Bid> Bids { get; set;} = [];
    public ICollection<RefreshToken> RefreshTokens { get; set;} = [];
    public ICollection<Transaction> Transactions { get; set;} = [];
    public ICollection<Notification> Notifications { get; set;} = [];
}
