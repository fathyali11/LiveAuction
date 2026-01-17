using LiveAuction.Shared.DTOs;
using Refit;

namespace LiveAuction.Client.Features.Notifications.Services;

public interface INotificationsApi
{
    [Get("/api/notifications")]
    Task<HttpResponseMessage> GetNotificationsAsync();

    [Get("/api/notifications/unread/count")]
    Task<HttpResponseMessage> GetUnreadCountAsync();

    [Post("/api/notifications/mark-as-read/{id}")]
    Task<HttpResponseMessage> MarkAsReadAsync(int id);

    [Post("/api/notifications/read-all")]
    Task<HttpResponseMessage> MarkAllAsReadAsync();
}
