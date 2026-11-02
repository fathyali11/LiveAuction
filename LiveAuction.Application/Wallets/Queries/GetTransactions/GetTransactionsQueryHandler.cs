using LiveAuction.Application.Services;
using LiveAuction.Domain.Consts;
using LiveAuction.Shared.DTOs;
using MediatR;
using Microsoft.Extensions.Logging;
using OneOf;

namespace LiveAuction.Application.Wallets.Queries.GetTransactions;

internal class GetTransactionsQueryHandler(
    IWalletService _walletService,
    ILogger<GetTransactionsQueryHandler> _logger
    ) : IRequestHandler<GetTransactionsQuery, OneOf<Error, PaginatedResult<TransactionResponse>>>
{
    public async Task<OneOf<Error, PaginatedResult<TransactionResponse>>> Handle(GetTransactionsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling GetTransactionsQuery for UserId: {UserId}, PageNumber: {PageNumber}, PageSize: {PageSize}", request.UserId, request.PaginatedRequest.PageNumber, request.PaginatedRequest.PageSize);

        var transactions = await _walletService.GetAllTransactionsAsync(request.UserId, request.PaginatedRequest, cancellationToken);
        _logger.LogInformation("Retrieved {Count} transactions for UserId: {UserId}", transactions.Items.Count, request.UserId);
        return transactions;
    }
}
