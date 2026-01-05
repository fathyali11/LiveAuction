using Blazored.LocalStorage;
using LiveAuction.Client.Features.Users.Services;
using LiveAuction.Shared.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Threading;

namespace LiveAuction.Client.Services.TokenServices;

public class TokenService(ILocalStorageService _localStorageService,
    IAuthApi _authApi) : ITokenService
{
    public async Task<string?> GetAccessTokenAsync()
    {
        var accessToken = await _localStorageService.GetItemAsync<string>("authToken");
        if (string.IsNullOrEmpty(accessToken))
            return null;

        if (!IsTokenExpired(accessToken))
            return accessToken;
        try
        {

            var refreshToken = await _localStorageService.GetItemAsync<string>("refreshToken");
            if (string.IsNullOrEmpty(refreshToken))
            {
                await _localStorageService.RemoveItemAsync("authToken");
                return null;
            }
            var resfreshResponse = await _authApi.RefreshTokenAsync(new RefreshTokenRequest { Token = accessToken, RefreshToken = refreshToken });
            if (!resfreshResponse.IsSuccessStatusCode)
            {
                await _localStorageService.RemoveItemAsync("authToken");
                await _localStorageService.RemoveItemAsync("refreshToken");
                return null;
            }

            var result = await resfreshResponse.Content.ReadFromJsonAsync<AuthResponse>();
            if (result is null)
                return null;

            await _localStorageService.SetItemAsync("authToken", result.Token);
            await _localStorageService.SetItemAsync("refreshToken", result.RefreshToken);

            return result?.Token;
        }
        catch
        {
            return null;
        }
    }

    private static bool IsTokenExpired(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            return jwtToken.ValidTo < DateTime.UtcNow.AddMinutes(-10);
        }
        catch
        {
            return true;
        }
    }
}
