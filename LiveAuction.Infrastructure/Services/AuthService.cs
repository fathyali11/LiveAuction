using LiveAuction.Domain.Consts;
using LiveAuction.Domain.Entities;
using LiveAuction.Domain.Services;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace LiveAuction.Infrastructure.Services;

internal class AuthService(IOptions<JwtSettings> options) : IAuthService
{
    private readonly JwtSettings _jwtSettings = options.Value;
    public async Task<(string,DateTime)> GenerateJwtTokenAsync(ApplicationUser user, IList<string> roles, CancellationToken cancellationToken = default)
    {
        var claims = new List<Claim>
        {
            new (JwtRegisteredClaimNames.Sub, user.Id),
            new (JwtRegisteredClaimNames.Email, user.Email!),
            new (JwtRegisteredClaimNames.Name, user.FullName!),
            new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new ("roles", JsonSerializer.Serialize(roles))
        };
        var expiration = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryInMinutes);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims:claims,
            expires: expiration,
            signingCredentials: creds
        );
        var tokenHandler = new JwtSecurityTokenHandler().WriteToken(token);
        return (tokenHandler, expiration);
    }

}
