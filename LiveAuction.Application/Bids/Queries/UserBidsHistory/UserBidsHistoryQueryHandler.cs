using LiveAuction.Domain.Consts;
using LiveAuction.Domain.Repositories;
using LiveAuction.Shared.DTOs;
using MediatR;
using Microsoft.Extensions.Logging;
using OneOf;

namespace LiveAuction.Application.Bids.Queries.UserBidsHistory;

internal class UserBidsHistoryQueryHandler(
    IBidRepository _bidRepository,
    ILogger<UserBidsHistoryQueryHandler> _logger
    ) : IRequestHandler<UserBidsHistoryQuery, OneOf<Error, List<UserBidDto>>>

{
    public async Task<OneOf<Error, List<UserBidDto>>> Handle(UserBidsHistoryQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling UserBidsHistoryQuery for UserId: {UserId}", request.UserId);
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
