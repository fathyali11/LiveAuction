using LiveAuction.Domain.Entities;
using LiveAuction.Domain.Repositories;
using LiveAuction.Infrastructure.Presistence;
using Microsoft.EntityFrameworkCore;

namespace LiveAuction.Infrastructure.Repositories;

internal class RefreshTokenRepository(ApplicationDbContext _context): IRefreshTokenRepository
{
    public async Task<RefreshToken?> IsActiveAsync(string token, CancellationToken cancellationToken = default)
    {
        var tokens = await _context.RefreshTokens
            .FirstOrDefaultAsync(x=>x.Token==token,cancellationToken);
        return tokens;
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
}
