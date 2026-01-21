using LiveAuction.Shared.DTOs;
using Refit;

namespace LiveAuction.Client.Features.Notifications.Services;

public interface INotificationsApi
{
    [Get("/api/notifications")]
    Task<HttpResponseMessage> GetNotificationsAsync();

    [Get("/api/notifications/unread/count")]
    Task<HttpResponseMessage> GetUnreadCountAsync();

    [Put("/api/notifications/mark-as-read/{id}")]
    Task<HttpResponseMessage> MarkAsReadAsync(int id);

    [Put("/api/notifications/mark-all-as-read")]
    Task<HttpResponseMessage> MarkAllAsReadAsync();
}
