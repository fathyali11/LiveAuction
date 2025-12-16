using FluentValidation;
using LiveAuction.Application.ApplicationUsers.Commands.LoginUser;
using LiveAuction.Application.ApplicationUsers.Commands.RegisterUser;
using LiveAuction.Application.Auctions.Commands.CreateAuction;
using LiveAuction.Application.Auctions.Commands.DeleteAuction;
using LiveAuction.Application.Auctions.Commands.UpdateAuction;
using LiveAuction.Application.Auctions.Queries.GetAuctionById;
using LiveAuction.Application.Bids.Commands.CreateBid;
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
        services.AddScoped<IValidator<CreateAuctionCommand>, CreateAuctionCommandValidator>();
        services.AddScoped<IValidator<UpdateAuctionCommand>, UpdateAuctionCommandValidator>();
        services.AddScoped<IValidator<DeleteAuctionCommand>, DeleteAuctionCommandValidator>();
        services.AddScoped<IValidator<GetAuctionByIdQuery>, GetAuctionByIdQueryValidator>();
        services.AddScoped<IValidator<CreateBidCommand>, CreateBidCommandValidator>();

        TypeAdapterConfig.GlobalSettings.Scan(assembly);
        services.AddMapster();

        return services;
    }
}
