using LiveAuction.Shared.Enums;

namespace LiveAuction.Domain.Consts;

public record CreateNotificationDto(
    string UserId, 
    string Title,
    string Message, 
    NotificationType NotificationType, 
    int RelatedEntityId);
