using FluentValidation;
using Hangfire;
using LiveAuction.Application.ApplicationUsers.Commands.LoginUser;
using LiveAuction.Application.ApplicationUsers.Commands.RefreshToken;
using LiveAuction.Application.ApplicationUsers.Commands.RegisterUser;
using LiveAuction.Application.Auctions.Commands.CreateAuction;
using LiveAuction.Application.Auctions.Commands.DeleteAuction;
using LiveAuction.Application.Auctions.Commands.UpdateAuction;
using LiveAuction.Application.Auctions.Queries.GetAllAuctions;
using LiveAuction.Application.Auctions.Queries.GetAuctionById;
using LiveAuction.Application.Bids.Commands.CreateBid;
using LiveAuction.Application.Bids.Queries.UserBidsHistory;
using LiveAuction.Application.Services.AuctionServices;
using LiveAuction.Application.Services.AuthServices;
using LiveAuction.Application.Services.BackgroundJobServices;
using LiveAuction.Application.Services.WalletServices;
using LiveAuction.Application.Wallets.Commands.Deposit;
using LiveAuction.Application.Wallets.Queries.GetTransactions;
using LiveAuction.Application.Wallets.Queries.GetWalletSummary;
using LiveAuction.Application.WatchLists.Commands.ToggleWatchListItem;
using LiveAuction.Domain.Consts;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using System.Text;

namespace LiveAuction.Application.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

        services.AddScoped<IBackgroundJobService, BackgroundJobService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAuctionService, AuctionService>();
        services.AddScoped<IWalletService, WalletService>();

        services.AddScoped<IValidator<RegisterUserCommand>, RegisterUserCommandValidator>();
        services.AddScoped<IValidator<LoginUserCommand>, LoginUserCommandValidator>();
        services.AddScoped<IValidator<CreateAuctionCommand>, CreateAuctionCommandValidator>();
        services.AddScoped<IValidator<UpdateAuctionCommand>, UpdateAuctionCommandValidator>();
        services.AddScoped<IValidator<DeleteAuctionCommand>, DeleteAuctionCommandValidator>();
        services.AddScoped<IValidator<GetAuctionByIdQuery>, GetAuctionByIdQueryValidator>();
        services.AddScoped<IValidator<CreateBidCommand>, CreateBidCommandValidator>();
        services.AddScoped<IValidator<RefreshTokenCommand>, RefreshTokenCommandValidator>();
        services.AddScoped<IValidator<UserBidsHistoryQuery>, UserBidsHistoryQueryValidator>();
        services.AddScoped<IValidator<ToggleWatchListItemCommand>, ToggleWatchListItemCommandValidator>();
        services.AddScoped<IValidator<DepositCommand>, DepositCommandValidator>();
        services.AddScoped<IValidator<GetWalletSummaryQuery>, GetWalletSummaryQueryValidator>();
        services.AddScoped<IValidator<GetTransactionsQuery>, GetTransactionsQueryValidator>();
        services.AddScoped<IValidator<GetAllAuctionsQuery>, GetAllAuctionsQueryValidator>();
        TypeAdapterConfig.GlobalSettings.Scan(assembly);
        services.AddMapster();
        var connectionString = configuration.GetConnectionString("LiveAuctionDbConnection");
        services.AddHangfire(configuration => configuration
            .UseSqlServerStorage(connectionString));

        services.AddHangfireServer();

        services.AddOptions<JwtSettings>()
            .Bind(configuration.GetSection(nameof(JwtSettings)))
            .ValidateDataAnnotations()
            .ValidateOnStart();


        var jwtSettings = configuration.GetSection(nameof(JwtSettings)).Get<JwtSettings>();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
            .AddJwtBearer(options =>
            {
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];

                        var path = context.HttpContext.Request.Path;

                        if (!string.IsNullOrEmpty(accessToken) &&
                            (path.StartsWithSegments("/hubs"))) 
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings!.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                    ClockSkew = TimeSpan.FromMinutes(5)
                };
            });

        


        return services;
    }
}
