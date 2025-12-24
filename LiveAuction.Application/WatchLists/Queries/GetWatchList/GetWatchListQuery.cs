using LiveAuction.Domain.Consts;
using LiveAuction.Shared.DTOs;
using MediatR;
using OneOf;

namespace LiveAuction.Application.WatchLists.Queries.GetWatchList;

public record GetWatchListQuery(string UserId) : IRequest<OneOf<Error, WatchListDto>>;