using LiveAuction.API.Exceptions;
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
                    // 👇 هنا بنكتب رابط الفرونت إند بتاعك بالظبط (خده من اللوج copy paste)
                    .WithOrigins("https://localhost:7085", "http://localhost:5000")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()); // مهمة عشان الـ Auth و SignalR
        });
        builder.Services.AddControllers();
        return builder;
    }
}
