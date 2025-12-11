using Mapster;
using LiveAuction.Application.ApplicationUsers.Commands.RegisterUser;
using LiveAuction.Domain.Entities;

namespace LiveAuction.Application.ApplicationUsers.Dtos;

internal class UserMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<RegisterUserCommand, ApplicationUser>()
            .Map(dest => dest.UserName, src => src.Email)
            .Map(dest => dest.Email, src => src.Email);
    }
}
