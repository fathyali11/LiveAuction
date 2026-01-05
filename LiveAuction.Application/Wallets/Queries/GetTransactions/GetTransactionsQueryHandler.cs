using FluentValidation;
using LiveAuction.Application.Services.WalletServices;
using LiveAuction.Domain.Consts;
using LiveAuction.Shared.DTOs;
using MediatR;
using Microsoft.Extensions.Logging;
using OneOf;

namespace LiveAuction.Application.Wallets.Queries.GetTransactions;

internal class GetTransactionsQueryHandler(
    IWalletService _walletService,
    IValidator<GetTransactionsQuery> _validator,
    ILogger<GetTransactionsQueryHandler> _logger
    ) : IRequestHandler<GetTransactionsQuery, OneOf<Error, PaginatedResult<TransactionResponse>>>
{
    public async Task<OneOf<Error, PaginatedResult<TransactionResponse>>> Handle(GetTransactionsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling GetTransactionsQuery for UserId: {UserId}, PageNumber: {PageNumber}, PageSize: {PageSize}", request.UserId, request.PageNumber, request.PageSize);
        
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Validation failed for GetTransactionsQuery: {Errors}", validationResult.Errors);
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            return new Error(ErrorCodes.ValidationError,errors);
        }
        var transactions = await _walletService.GetAllTransactionsAsync(request.UserId, request.PageNumber, request.PageSize, cancellationToken);
        _logger.LogInformation("Retrieved {Count} transactions for UserId: {UserId}", transactions.Items.Count, request.UserId);
        return transactions;
    }
}
