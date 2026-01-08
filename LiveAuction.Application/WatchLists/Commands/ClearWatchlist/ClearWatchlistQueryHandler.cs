using FluentValidation;
using LiveAuction.Application.Services.WatchlistServices;
using LiveAuction.Domain.Consts;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LiveAuction.Application.WatchLists.Commands.ClearWatchlist;

internal class ClearWatchlistQueryHandler(
    IWatchlistService _watchlistService,
    ILogger<ClearWatchlistQueryHandler> _logger,
    IValidator<ClearWatchlistQuery> _validator
    ) : IRequestHandler<ClearWatchlistQuery, Error>
{
    public async Task<Error> Handle(ClearWatchlistQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling ClearWatchlistQuery for UserId: {UserId}", request.UserId);
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errorMessage = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            _logger.LogWarning("Validation failed for ClearWatchlistQuery: {Errors}", errorMessage);
            return new Error(ErrorCodes.ValidationError, errorMessage);
        }
        var isCleared = await _watchlistService.ClearWatchlistAsync(request.UserId, cancellationToken);
        if (!isCleared)
        {
            _logger.LogError("Failed to clear watchlist for UserId: {UserId}", request.UserId);
            return new Error(ErrorCodes.InternalServerError, "Failed to clear watchlist.");
        }
        _logger.LogInformation("Successfully cleared watchlist for UserId: {UserId}", request.UserId);
        return new Error(ErrorCodes.None, "Watchlist cleared successfully.");
    }
}
