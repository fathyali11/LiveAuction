using CleanArchitectureSolutionName.API.Exceptions;
using Serilog;

namespace CleanArchitectureSolutionName.API.Extensions;

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

        builder.Services.AddControllers();
        return builder;
    }
}
