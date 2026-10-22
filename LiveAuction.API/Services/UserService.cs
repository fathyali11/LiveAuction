using System.Security.Claims;
using LiveAuction.Application.Interfaces;

namespace LiveAuction.API.Services;

internal class UserService(IHttpContextAccessor _httpContextAccessor) : IUserService
{
    public string? GetCurrentUserId()
        => _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
}
