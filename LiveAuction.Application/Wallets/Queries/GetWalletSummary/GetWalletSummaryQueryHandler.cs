using FluentValidation;
using LiveAuction.Domain.Consts;
using LiveAuction.Domain.Repositories;
using LiveAuction.Shared.DTOs;
using MediatR;
using Microsoft.Extensions.Logging;
using OneOf;

namespace LiveAuction.Application.Wallets.Queries.GetWalletSummary;

internal class GetWalletSummaryQueryHandler(IWalletRepository _walletRepository,
    IValidator<GetWalletSummaryQuery> _validator,
    ILogger<GetWalletSummaryQueryHandler> _logger) : IRequestHandler<GetWalletSummaryQuery, OneOf<Error, WalletSummaryResponse>>
{
    public async Task<OneOf<Error, WalletSummaryResponse>> Handle(GetWalletSummaryQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling GetWalletSummaryQuery for UserId: {UserId}", request.UserId);
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errorMessage = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            _logger.LogWarning("Validation failed for GetWalletSummaryQuery: {ErrorMessage}", errorMessage);
            return new Error(ErrorCodes.ValidationError, errorMessage);
        }
        var wallet = await _walletRepository.GetWalletSummaryAsync(request.UserId, cancellationToken);
        if (wallet == null)
        {
            _logger.LogWarning("Wallet not found for UserId: {UserId}", request.UserId);
            return new Error(ErrorCodes.NotFoundError, "Wallet not found.");
        }
        _logger.LogInformation("Successfully retrieved wallet summary for UserId: {UserId}", request.UserId);
        return wallet;
    }
}
