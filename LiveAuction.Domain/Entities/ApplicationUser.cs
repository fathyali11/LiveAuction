using Microsoft.AspNetCore.Identity;

namespace LiveAuction.Domain.Entities;

public class ApplicationUser:IdentityUser
{
    public string FullName { get; set; }=string.Empty;
    public ICollection<Bid> Bids { get; set;} = [];
    public ICollection<RefreshToken> RefreshTokens { get; set;} = [];
}
