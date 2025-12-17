using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

namespace LiveAuction.Client.Auth;

public class CustomAuthStateProvider(
    ILocalStorageService _localStorageService,
    HttpClient _httpClient
    ) : AuthenticationStateProvider
{
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token =await _localStorageService.GetItemAsync<string>("authToken");

        if(string.IsNullOrWhiteSpace(token))
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt")));
    }

    public async Task MarkUserAsAuthenticated(string token,string refreshToken)
    {
        await _localStorageService.SetItemAsync("authToken", token);
        await _localStorageService.SetItemAsync("refreshToken", refreshToken);
        var authState = await GetAuthenticationStateAsync();
        NotifyAuthenticationStateChanged(Task.FromResult(authState));
    }
    public async Task MarkUserAsLoggedOut()
    {
        await _localStorageService.RemoveItemAsync("authToken");
        _httpClient.DefaultRequestHeaders.Authorization = null;
        var authState = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        NotifyAuthenticationStateChanged(Task.FromResult(authState));
    }

    private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var payload = jwt.Split('.')[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

        var claims = new List<Claim>();

        foreach (var kvp in keyValuePairs)
        {
            if (kvp.Key == "role" || kvp.Key == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
            {
                if (kvp.Value.ToString().Trim().StartsWith("["))
                {
                    var parsedRoles = JsonSerializer.Deserialize<string[]>(kvp.Value.ToString() ?? string.Empty);
                    foreach (var parsedRole in parsedRoles)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, parsedRole));
                    }
                }
                else
                {
                    claims.Add(new Claim(ClaimTypes.Role, kvp.Value.ToString()??string.Empty));
                }
            }
            else if (kvp.Key == "name")
            {
                claims.Add(new Claim(ClaimTypes.Name, kvp.Value.ToString()??string.Empty)); 
                claims.Add(new Claim(kvp.Key, kvp.Value.ToString() ?? string.Empty)); 
            }
            else
            {
                claims.Add(new Claim(kvp.Key, kvp.Value.ToString() ?? string.Empty));
            }
        }

        return claims;
    }

    private byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }
}
