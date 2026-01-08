using LiveAuction.Shared.Enums;

namespace LiveAuction.Domain.Entities;

public class Notification
{
    public int Id { get; set; }
    public string UserId { get; set; }= string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }= DateTime.UtcNow;
    public bool IsRead { get; set; }= false;
    public NotificationType NotificationType { get; set; }
    public int? RelatedEntityId { get; set; }

    public ApplicationUser User { get; set; } = null!;
}