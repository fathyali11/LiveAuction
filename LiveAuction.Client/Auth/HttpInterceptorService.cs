using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using System.Net;

namespace LiveAuction.Client.Auth;

public class HttpInterceptorService(IServiceProvider serviceProvider,
    ISnackbar snackbar,
    NavigationManager navigationManager):DelegatingHandler
{
    override protected async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            snackbar.Add("You are not authorized to perform this action. Please log in.", Severity.Error);
            var authStateProvider = serviceProvider.GetRequiredService<AuthenticationStateProvider>() as CustomAuthStateProvider;
            if(authStateProvider is not null)
                await authStateProvider.MarkUserAsLoggedOut();

            navigationManager.NavigateTo("authentication/login");
        }
        else if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            snackbar.Add("You do not have permission to perform this action.", Severity.Warning);
            navigationManager.NavigateTo("/");
        }
        return response;
    }
}
