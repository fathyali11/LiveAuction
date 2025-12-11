using FluentValidation;
using LiveAuction.Application.ApplicationUsers.Commands.LoginUser;
using LiveAuction.Application.ApplicationUsers.Commands.RegisterUser;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace LiveAuction.Application.Extensions;
public static class ServiceCollectionExtension
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddScoped<IValidator<RegisterUserCommand>, RegisterUserCommandValidator>();
        services.AddScoped<IValidator<LoginUserCommand>, LoginUserCommandValidator>();

        
        TypeAdapterConfig.GlobalSettings.Scan(assembly);
        services.AddMapster();
        return services;
    }
}
