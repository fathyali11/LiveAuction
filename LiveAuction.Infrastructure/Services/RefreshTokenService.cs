using LiveAuction.Application.Services;
using LiveAuction.Domain.Entities;
using LiveAuction.Domain.Repositories;

namespace LiveAuction.Infrastructure.Services;

internal class RefreshTokenService(IRefreshTokenRepository _refreshTokenRepository): IRefreshTokenService
{
    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _refreshTokenRepository.GetByTokenAsync(token, cancellationToken);
    }
    public async Task UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        await _refreshTokenRepository.UpdateAsync(refreshToken, cancellationToken);
    }
    public async Task RemoveAllAsync(string userId, CancellationToken cancellationToken = default)
    {
        await _refreshTokenRepository.RemoveAllAsync(userId, cancellationToken);
    }
    public async Task RemoveInactiveAsync(string userId, CancellationToken cancellationToken = default)
    {
        await _refreshTokenRepository.RemoveInactiveAsync(userId, cancellationToken);
    }
    public async Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
    }
    public async Task RotateAsync(RefreshToken currentToken, RefreshToken newToken, CancellationToken cancellationToken = default)
    {
        await _refreshTokenRepository.RotateAsync(currentToken, newToken, cancellationToken);
    }
}
