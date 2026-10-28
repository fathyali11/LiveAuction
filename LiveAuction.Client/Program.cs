using Blazored.LocalStorage;
using LiveAuction.Client;
using LiveAuction.Client.Auth;
using LiveAuction.Client.Consts;
using LiveAuction.Client.Features.Auctions.Services;
using LiveAuction.Client.Features.Bids.Services;
using LiveAuction.Client.Features.Notifications.Services;
using LiveAuction.Client.Features.Users.Services;
using LiveAuction.Client.Features.Wallets.Services;
using LiveAuction.Client.Features.Watchlists.Services;
using LiveAuction.Client.Services.TokenServices;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using Refit;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Same origin by default (the API hosts the client). When the client runs on its
// own dev server, wwwroot/appsettings.Development.json overrides it with the API URL.
var apiBaseUrl = builder.Configuration["ApiBaseUrl"];
HostUrl.Current = string.IsNullOrWhiteSpace(apiBaseUrl)
    ? builder.HostEnvironment.BaseAddress.TrimEnd('/')
    : apiBaseUrl.TrimEnd('/');

builder.Services.AddScoped<HttpInterceptorService>();

builder.Services.AddRefitGeneratedClient<IAuctionsApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(($"{HostUrl.Current}")))
    .AddHttpMessageHandler<HttpInterceptorService>();

builder.Services.AddRefitGeneratedClient<IAuthApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(($"{HostUrl.Current}")));

builder.Services.AddRefitGeneratedClient<IWatchlistApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(($"{HostUrl.Current}")))
    .AddHttpMessageHandler<HttpInterceptorService>();

builder.Services.AddRefitGeneratedClient<IBidsApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(($"{HostUrl.Current}")))
    .AddHttpMessageHandler<HttpInterceptorService>();

builder.Services.AddRefitGeneratedClient<IWalletApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(($"{HostUrl.Current}")))
    .AddHttpMessageHandler<HttpInterceptorService>();

builder.Services.AddRefitGeneratedClient<INotificationsApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(($"{HostUrl.Current}")))
    .AddHttpMessageHandler<HttpInterceptorService>();



builder.Services.AddMudServices();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider,CustomAuthStateProvider>();
builder.Services.AddScoped<ITokenService, TokenService>();

await builder.Build().RunAsync();
