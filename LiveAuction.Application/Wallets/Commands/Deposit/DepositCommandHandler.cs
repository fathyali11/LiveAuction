using FluentValidation;
using LiveAuction.Application.Interfaces;
using LiveAuction.Application.Services.WalletServices;
using LiveAuction.Domain.Consts;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LiveAuction.Application.Wallets.Commands.Deposit;

internal class DepositCommandHandler(IWalletService _walletService,
    IValidator<DepositCommand> _validator,
    ILogger<DepositCommandHandler>_logger,
    IAuctionNotificationService _auctionNotificationService)
    : IRequestHandler<DepositCommand, Error>
{
    public async Task<Error> Handle(DepositCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling DepositCommand for UserId: {UserId}, Amount: {Amount}", request.UserId, request.Amount);
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var firstError = validationResult.Errors.First();
            _logger.LogWarning("Validation failed for DepositCommand: {ErrorMessage}", firstError.ErrorMessage);
            return new Error(ErrorCodes.ValidationError, firstError.ErrorMessage);
        }
        var isDeposited = await _walletService.DepositAsync(request.UserId, request.Amount, cancellationToken);
        if (!isDeposited)
        {
            _logger.LogError("Deposit failed for UserId: {UserId}, Amount: {Amount}", request.UserId, request.Amount);
            return new Error(ErrorCodes.InternalServerError, "Deposit operation failed.");
        }
        _logger.LogInformation("Deposit successful for UserId: {UserId}, Amount: {Amount}", request.UserId, request.Amount);
        return new Error(ErrorCodes.None, "Deposit successful.");
    }
}
