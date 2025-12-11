using LiveAuction.Domain.Entities;

namespace LiveAuction.Domain.Services;

public interface IAuthService
{
    Task<(string, DateTime)> GenerateJwtTokenAsync(ApplicationUser user, IList<string> roles, CancellationToken cancellationToken = default);
}
