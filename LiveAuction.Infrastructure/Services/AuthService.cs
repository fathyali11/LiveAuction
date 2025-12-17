using LiveAuction.Domain.Consts;
using LiveAuction.Domain.Entities;
using LiveAuction.Domain.Services;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace LiveAuction.Infrastructure.Services;

internal class AuthService(IOptions<JwtSettings> options) : IAuthService
{
    private readonly JwtSettings _jwtSettings = options.Value;
    public async Task<CreatedTokenDto> GenerateJwtTokenAsync(ApplicationUser user, IList<string> roles, CancellationToken cancellationToken = default)
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
        var refreshToken = GenerateRefreshToken();
        return new CreatedTokenDto
        {
            Token = tokenHandler,
            Expiration = expiration,
            RefreshToken = refreshToken,
            RefreshTokenExpiration = expiration.AddDays(_jwtSettings.RefreshTokenExpiryInDays)
        };
    }
    public async Task<string?> GetUserIdFrom(string token, CancellationToken cancellationToken = default)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);
        try
        {
            tokenHandler.InboundClaimTypeMap.Clear();
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidAudience = _jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(key)
            }, out SecurityToken validatedToken);
            var userIdClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub);
            return userIdClaim?.Value;
        }
        catch
        {
            return null;
        }
    }
    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

}
