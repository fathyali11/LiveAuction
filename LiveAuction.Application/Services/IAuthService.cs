using LiveAuction.Domain.Consts;
using LiveAuction.Domain.Entities;
using Microsoft.EntityFrameworkCore.Storage;

namespace LiveAuction.Application.Services;

public interface IAuthService
{
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task<CreatedTokenDto> GenerateJwtTokenAsync(ApplicationUser user, IList<string> roles, CancellationToken cancellationToken = default);
    Task<string?> GetUserIdFrom(string token, CancellationToken cancellationToken = default);
}
