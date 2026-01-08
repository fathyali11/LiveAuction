using LiveAuction.Shared.DTOs;
using Refit;

namespace LiveAuction.Client.Features.Wallets.Services;

public interface IWalletApi
{
    [Post("/api/wallets/deposit")]
    Task<HttpResponseMessage> DepositAsync([Body] DepositRequest request, CancellationToken cancellationToken = default);

    [Get("/api/wallets/summary")]
    Task<HttpResponseMessage> GetWalletSummaryAsync(CancellationToken cancellationToken = default);

    [Get("/api/wallets/transactions")]
    Task<HttpResponseMessage> GetTransactionsAsync([Query] PaginatedRequest paginatedRequest, CancellationToken cancellationToken = default);
}
