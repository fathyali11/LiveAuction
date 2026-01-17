using LiveAuction.Domain.Consts;
using MediatR;
using OneOf;

namespace LiveAuction.Application.Notifications.Queries.GetCountUnRead;

public record GetCountUnReadQuery(string UserId): IRequest<OneOf<Error, int>>;
