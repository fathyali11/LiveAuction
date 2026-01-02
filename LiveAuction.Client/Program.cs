using Blazored.LocalStorage;
using LiveAuction.Client;
using LiveAuction.Client.Auth;
using LiveAuction.Client.Features.Auctions.Services;
using LiveAuction.Client.Features.Bids.Services;
using LiveAuction.Client.Features.Users.Services;
using LiveAuction.Client.Features.Wallets.Services;
using LiveAuction.Client.Features.Watchlists.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using Refit;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped<HttpInterceptorService>();

builder.Services.AddRefitClient<IAuctionsApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(("https://localhost:7293")))
    .AddHttpMessageHandler<HttpInterceptorService>();

builder.Services.AddRefitClient<IAuthApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(("https://localhost:7293")));

builder.Services.AddRefitClient<IWatchlistApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(("https://localhost:7293")))
    .AddHttpMessageHandler<HttpInterceptorService>();

builder.Services.AddRefitClient<IBidsApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(("https://localhost:7293")))
    .AddHttpMessageHandler<HttpInterceptorService>();

builder.Services.AddRefitClient<IWalletApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(("https://localhost:7293")))
    .AddHttpMessageHandler<HttpInterceptorService>();



builder.Services.AddMudServices();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider,CustomAuthStateProvider>();

await builder.Build().RunAsync();
