using LiveAuction.Application.Interfaces;
using LiveAuction.Application.Services;
using LiveAuction.Domain.Entities;
using LiveAuction.Domain.Repositories;
using LiveAuction.Infrastructure.Presistence;
using LiveAuction.Infrastructure.Repositories;
using LiveAuction.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LiveAuction.Infrastructure.Extensions;
public static class ServiceCollectionExtension
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,IConfiguration configuration)
    {
        services.AddScoped<IAuctionRepository,AuctionRepository>();
        services.AddScoped<IBidRepository,BidRepository>();
        services.AddScoped<IRefreshTokenRepository,RefreshTokenRepository>();
        services.AddScoped<IWatchListRepository,WatchListRepository>();
        services.AddScoped<IWalletRepository,WalletRepository>();
        services.AddScoped<INotificationRepository,NotificationRepository>();
        services.AddScoped<IFileService,LocalFileService>();

        services.AddScoped<IBackgroundJobService, BackgroundJobService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAuctionService, AuctionService>();
        services.AddScoped<IWalletService, WalletService>();
        services.AddScoped<IWatchlistService, WatchlistService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IBidService, BidService>();

        var connectionString = configuration.GetConnectionString("LiveAuctionDbConnection");
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });

        services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();


        return services;


    }
}
