using Microsoft.Extensions.DependencyInjection;

namespace LiveAuction.Infrastructure.Extensions;
public static class ServiceCollectionExtension
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        // Add infrastructure services here
        //services.AddScoped<IUnitOfWork, UnitOfWork>();
        return services;
    }
}
