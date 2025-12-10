using Microsoft.Extensions.DependencyInjection;

namespace LiveAuction.Application.Extensions;
public static class ServiceCollectionExtension
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Add application services here
        return services;
    }
}
