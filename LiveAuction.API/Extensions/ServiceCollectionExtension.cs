using System.Security.Claims;
using System.Threading.RateLimiting;
using LiveAuction.API.Exceptions;
using LiveAuction.API.Services;
using LiveAuction.Application.Interfaces;
using Serilog;

namespace LiveAuction.API.Extensions;

public static class ServiceCollectionExtension
{
    public static WebApplicationBuilder AddApiServices(this WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .CreateLogger();
        builder.Host.UseSerilog();

        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSwaggerGen();
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowBlazorClient",
                policy => policy
                    .WithOrigins(
                        "https://localhost:7085", // client https profile
                        "http://localhost:5270")  // client http profile
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()); 
        });
        builder.Services.AddControllers();
        builder.Services.AddSignalR();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<IAuctionNotificationService, AuctionNotificationService>();
        builder.Services.AddScoped<IUserService, UserService>();

        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy("create-auction", context =>
            {
                var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                var partitionKey = userId ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous";

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: partitionKey,
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    });
            });

            options.AddPolicy("place-bid", context =>
            {
                var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                var partitionKey = userId ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous";

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: $"bid:{partitionKey}",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromSeconds(10),
                        QueueLimit = 0
                    });
            });
        });
        return builder;
    }
}
