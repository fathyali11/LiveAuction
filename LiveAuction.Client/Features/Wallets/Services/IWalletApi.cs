using LiveAuction.Shared.DTOs;
using Refit;

namespace LiveAuction.Client.Features.Wallets.Services;

public interface IWalletApi
{
    [Post("/api/wallets/deposit")]
    Task<HttpResponseMessage> DepositAsync([Body] DepositRequest request, CancellationToken cancellationToken = default);
}
