using LiveAuction.Domain.Consts;
using LiveAuction.Shared.DTOs;
using MediatR;
using OneOf;

namespace LiveAuction.Application.WatchLists.Commands.ToggleWatchListItem;

public record ToggleWatchListItemCommand(
    string UserId,
    int AuctionId,
    string Title,
    string ImageName,
    decimal CurrentPrice,
    DateTime EndTime
    ): IRequest<OneOf<Error, ToggleWatchListItemResponse>>;