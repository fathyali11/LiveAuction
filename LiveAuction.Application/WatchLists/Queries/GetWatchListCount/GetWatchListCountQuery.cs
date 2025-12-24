using MediatR;

namespace LiveAuction.Application.WatchLists.Queries.GetWatchListCount;

public record GetWatchListCountQuery(string UserId) : IRequest<int>;