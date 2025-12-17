using LiveAuction.Domain.Consts;
using LiveAuction.Domain.Entities;

namespace LiveAuction.Domain.Services;

public interface IAuthService
{
    Task<CreatedTokenDto> GenerateJwtTokenAsync(ApplicationUser user, IList<string> roles, CancellationToken cancellationToken = default);
    Task<string?> GetUserIdFrom(string token, CancellationToken cancellationToken = default);
}
