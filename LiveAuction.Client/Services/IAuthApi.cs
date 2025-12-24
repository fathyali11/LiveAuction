using LiveAuction.Shared.DTOs;
using Refit;

namespace LiveAuction.Client.Services;

public interface IAuthApi 
{
    [Post("/api/auths/register")]
    Task<HttpResponseMessage> RegisterAsync([Body] RegisterRequest request, CancellationToken cancellationToken = default);
    
    [Post("/api/auths/login")]
    Task<HttpResponseMessage> LoginAsync([Body] LoginRequest request, CancellationToken cancellationToken = default);
    
    [Post("/api/auths/refresh-token")]
    Task<HttpResponseMessage> RefreshTokenAsync([Body] RefreshTokenRequest request, CancellationToken cancellationToken = default);
}