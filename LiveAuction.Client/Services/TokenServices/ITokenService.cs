namespace LiveAuction.Client.Services.TokenServices;

public interface ITokenService
{
    Task<string?> GetAccessTokenAsync();
}
