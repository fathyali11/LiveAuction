using LiveAuction.Domain.Repositories;
using LiveAuction.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace LiveAuction.Infrastructure.Extensions;
public static class ServiceCollectionExtension
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddScoped<IAuctionRepository,AuctionRepository>();
        services.AddScoped<IBidRepository,BidRepository>();
        return services;
    }
}
