using LiveAuction.Domain.Entities;

namespace LiveAuction.Domain.Repositories;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token,CancellationToken cancellationToken=default);
    Task UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    Task RemoveAllAsync(string userId, CancellationToken cancellationToken = default);
    Task RemoveInactiveAsync(string userId, CancellationToken cancellationToken = default);
    Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes <paramref name="currentToken"/> and inserts <paramref name="newToken"/> in a
    /// single unit of work, so the two changes are committed atomically.
    /// </summary>
    Task RotateAsync(RefreshToken currentToken, RefreshToken newToken, CancellationToken cancellationToken = default);
}
