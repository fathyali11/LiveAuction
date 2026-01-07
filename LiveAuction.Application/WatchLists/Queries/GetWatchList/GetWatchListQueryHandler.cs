using FluentValidation;
using LiveAuction.Application.Services.WatchlistServices;
using LiveAuction.Domain.Consts;
using LiveAuction.Domain.Repositories;
using LiveAuction.Shared.DTOs;
using MediatR;
using Microsoft.Extensions.Logging;
using OneOf;

namespace LiveAuction.Application.WatchLists.Queries.GetWatchList;

internal class GetWatchListQueryHandler(
    IWatchlistService _watchlistService,
    IValidator<GetWatchListQuery> _validator,
    ILogger<GetWatchListQueryHandler> _logger) : IRequestHandler<GetWatchListQuery, OneOf<Error, PaginatedResult<WatchListItemDto>>>
{
    public async Task<OneOf<Error, PaginatedResult<WatchListItemDto>>> Handle(GetWatchListQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting watchlist for user {UserId}", request.UserId);
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if(!validationResult.IsValid)
        {
            var errorMessage = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            _logger.LogWarning("Validation failed for GetWatchListQuery: {Errors}", errorMessage);
            return new Error(ErrorCodes.ValidationError, errorMessage);
        }
        var result = await _watchlistService.GetWatchlistAsync(request.UserId, request.PageNumber, request.PageSize, cancellationToken);
        _logger.LogInformation("Successfully retrieved watchlist for user {UserId}", request.UserId);
        return result;
    }
}