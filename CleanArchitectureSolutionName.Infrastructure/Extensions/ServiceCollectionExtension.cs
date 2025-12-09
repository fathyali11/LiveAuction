using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitectureSolutionName.Infrastructure.Extensions;
public static class ServiceCollectionExtension
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        // Add infrastructure services here
        //services.AddScoped<IUnitOfWork, UnitOfWork>();
        return services;
    }
}
