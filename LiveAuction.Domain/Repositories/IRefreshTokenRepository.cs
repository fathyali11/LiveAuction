using LiveAuction.Domain.Entities;

namespace LiveAuction.Domain.Repositories;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> IsActiveAsync(string token,CancellationToken cancellationToken=default);
    Task UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    Task RemoveAllAsync(string userId, CancellationToken cancellationToken = default);
}