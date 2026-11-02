using LiveAuction.Application.Interfaces;
using LiveAuction.Application.Services;
using LiveAuction.Domain.Consts;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LiveAuction.Application.Wallets.Commands.Deposit;

internal class DepositCommandHandler(IWalletService _walletService,
    ILogger<DepositCommandHandler>_logger)
    : IRequestHandler<DepositCommand, Error>
{
    public async Task<Error> Handle(DepositCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling DepositCommand for UserId: {UserId}, Amount: {Amount}", request.UserId, request.Amount);
        var depositResult = await _walletService.DepositAsync(request.UserId, request.Amount, cancellationToken);
        if (!depositResult.success)
        {
            _logger.LogError("Deposit failed for UserId: {UserId}, Amount: {Amount}", request.UserId, request.Amount);
            return new Error(ErrorCodes.InternalServerError, "Deposit operation failed.");
        }
        _logger.LogInformation("Deposit successful for UserId: {UserId}, Amount: {Amount}", request.UserId, request.Amount);
        return new Error(ErrorCodes.None, "Deposit successful.");
    }
}
