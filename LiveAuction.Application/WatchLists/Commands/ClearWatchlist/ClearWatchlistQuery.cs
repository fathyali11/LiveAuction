using LiveAuction.Domain.Consts;
using MediatR;

namespace LiveAuction.Application.WatchLists.Commands.ClearWatchlist;

public record ClearWatchlistQuery(string UserId): IRequest<Error>;
