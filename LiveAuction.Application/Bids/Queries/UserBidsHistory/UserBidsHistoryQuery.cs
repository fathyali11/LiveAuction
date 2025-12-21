using LiveAuction.Domain.Consts;
using LiveAuction.Shared.DTOs;
using MediatR;
using OneOf;

namespace LiveAuction.Application.Bids.Queries.UserBidsHistory;

public record UserBidsHistoryQuery(string UserId): IRequest<OneOf<Error,List<UserBidDto>>>;
