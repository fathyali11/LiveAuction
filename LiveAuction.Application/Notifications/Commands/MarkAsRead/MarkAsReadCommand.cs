using LiveAuction.Domain.Consts;
using MediatR;
using OneOf;

namespace LiveAuction.Application.Notifications.Commands.MarkAsRead;
public record MarkAsReadCommand(string UserId,int NotificationId):IRequest<OneOf<Error,bool>>;
