using LiveAuction.Shared.Enums;

namespace LiveAuction.Shared.DTOs;

public record NotificationDto(
    string UserId, 
    string Title,
    string Message, 
    bool IsRead,
    NotificationType NotificationType, 
    int ?RelatedEntityId=null);
