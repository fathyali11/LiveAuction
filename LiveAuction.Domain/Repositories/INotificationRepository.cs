using LiveAuction.Domain.Entities;

namespace LiveAuction.Domain.Repositories;

public interface INotificationRepository
{
    Task AddAsync(Notification notification, CancellationToken cancellationToken = default);
}
