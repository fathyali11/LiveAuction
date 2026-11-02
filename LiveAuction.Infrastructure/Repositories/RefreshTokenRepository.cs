using LiveAuction.Domain.Entities;
using LiveAuction.Domain.Repositories;
using LiveAuction.Infrastructure.Presistence;
using Microsoft.EntityFrameworkCore;

namespace LiveAuction.Infrastructure.Repositories;

internal class RefreshTokenRepository(ApplicationDbContext _context): IRefreshTokenRepository
{
    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _context.RefreshTokens
            .FirstOrDefaultAsync(x=>x.Token==token,cancellationToken);
    }
    public async Task UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        _context.RefreshTokens.Update(refreshToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
    public async Task RemoveAllAsync(string userId, CancellationToken cancellationToken = default)
    {
        await _context.RefreshTokens
            .Where(x=>x.ApplicationUserId==userId)
            .ExecuteDeleteAsync(cancellationToken);
    }
    public async Task RemoveInactiveAsync(string userId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        await _context.RefreshTokens
            .Where(x => x.ApplicationUserId == userId && (x.RevokedAt != null || x.ExpiresAt <= now))
            .ExecuteDeleteAsync(cancellationToken);
    }
    public async Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        await _context.RefreshTokens.AddAsync(refreshToken,cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
    public async Task RotateAsync(RefreshToken currentToken, RefreshToken newToken, CancellationToken cancellationToken = default)
    {
        currentToken.RevokedAt = DateTime.UtcNow;
        await _context.RefreshTokens.AddAsync(newToken, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
