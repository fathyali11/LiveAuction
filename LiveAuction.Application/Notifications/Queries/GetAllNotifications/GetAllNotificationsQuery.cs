using LiveAuction.Domain.Consts;
using LiveAuction.Shared.DTOs;
using MediatR;
using OneOf;

namespace LiveAuction.Application.Notifications.Queries.GetAllNotifications;
public record GetAllNotificationsQuery(string UserId): IRequest<OneOf<Error, List<NotificationDto>>>;
