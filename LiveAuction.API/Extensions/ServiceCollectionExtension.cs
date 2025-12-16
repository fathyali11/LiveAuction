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
                    .WithOrigins("https://localhost:7085")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()); 
        });
        builder.Services.AddControllers();
        builder.Services.AddSignalR();
        builder.Services.AddScoped<IAuctionNotificationService, AuctionNotificationService>();
        return builder;
    }
}
