using LiveAuction.Shared.DTOs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Blazored.LocalStorage; 

namespace LiveAuction.Client.Auth;

public class HttpInterceptorService(
    IServiceProvider serviceProvider,
    ISnackbar snackbar,
    NavigationManager navigationManager,
    ILocalStorageService _localStorageService,
    IHttpClientFactory _httpClientFactory) 
    : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _localStorageService.GetItemAsync<string>("authToken");
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            var refreshToken = await _localStorageService.GetItemAsync<string>("refreshToken");
            var authClient = _httpClientFactory.CreateClient("AuthClient");

            var resfreshResponse = await authClient.PostAsJsonAsync("api/auths/refresh-token", new
            {
                Token = token,
                RefreshToken = refreshToken
            });

            if (resfreshResponse.IsSuccessStatusCode)
            {
                var result = await resfreshResponse.Content.ReadFromJsonAsync<AuthResponse>(cancellationToken: cancellationToken);

                if (result is not null)
                {
                    var authStateProvider = serviceProvider.GetRequiredService<AuthenticationStateProvider>() as CustomAuthStateProvider;
                    if (authStateProvider is not null)
                        await authStateProvider.MarkUserAsAuthenticated(result.Token,result.RefreshToken);

                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.Token);

                    return await base.SendAsync(request, cancellationToken);
                }
            }

            await ((CustomAuthStateProvider)serviceProvider.GetRequiredService<AuthenticationStateProvider>()).MarkUserAsLoggedOut();
            navigationManager.NavigateTo("authentication/login");
        }
        else if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            snackbar.Add("ليس لديك صلاحية للقيام بهذا الإجراء.", Severity.Warning);
            navigationManager.NavigateTo("/");
        }

        return response;
    }
}