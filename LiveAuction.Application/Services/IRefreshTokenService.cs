using LiveAuction.Domain.Entities;

namespace LiveAuction.Application.Services;

public interface IRefreshTokenService
{
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    Task RemoveAllAsync(string userId, CancellationToken cancellationToken = default);
    Task RemoveInactiveAsync(string userId, CancellationToken cancellationToken = default);
    Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    Task RotateAsync(RefreshToken currentToken, RefreshToken newToken, CancellationToken cancellationToken = default);
}
