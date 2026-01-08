using LiveAuction.Domain.Consts;
using System;
using System.Collections.Generic;
using System.Text;

namespace LiveAuction.Application.Services.NotificationServices;

internal interface INotificationService
{
    Task AddNotificationAsync(CreateNotificationDto createNotificationDto, CancellationToken cancellationToken = default);
}
