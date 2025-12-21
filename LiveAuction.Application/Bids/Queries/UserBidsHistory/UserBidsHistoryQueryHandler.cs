using FluentValidation;
using LiveAuction.Domain.Consts;
using LiveAuction.Domain.Repositories;
using LiveAuction.Shared.DTOs;
using MediatR;
using Microsoft.Extensions.Logging;
using OneOf;

namespace LiveAuction.Application.Bids.Queries.UserBidsHistory;

internal class UserBidsHistoryQueryHandler(
    IBidRepository _bidRepository,
    IValidator<UserBidsHistoryQuery> _validator,
    ILogger<UserBidsHistoryQueryHandler> _logger
    ) : IRequestHandler<UserBidsHistoryQuery, OneOf<Error, List<UserBidDto>>>

{
    public async Task<OneOf<Error, List<UserBidDto>>> Handle(UserBidsHistoryQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling UserBidsHistoryQuery for UserId: {UserId}", request.UserId);
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errorMessage = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            _logger.LogWarning("Validation failed for UserBidsHistoryQuery: {ErrorMessage}", errorMessage);
            return new Error(ErrorCodes.ValidationError, errorMessage);
        }
        var userBids = await _bidRepository.GetUserBidsHistoryAsync(request.UserId, cancellationToken);
        _logger.LogInformation("Retrieved {Count} bids for UserId: {UserId}", userBids.Count, request.UserId);
        if(userBids is null)
        {
            _logger.LogWarning("No bids found for UserId: {UserId}", request.UserId);
            return new Error(ErrorCodes.NotFoundError, "No bids found for the specified user.");
        }
        return userBids;
    }
}
