using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Business.Services.Security.Auth.Jwt.Interface;
using Business.Services.Security.Auth.Jwt.Models;
using Infrastructure.Data.Postgres.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shared.Models;
using Shared.Models.Configuration;

namespace Business.Services.Security.Auth.Jwt;

public class JwtTokenService : IJwtTokenService
{
    private readonly ConfigurationOptions.JwtOptions _jwtOptions;

    public JwtTokenService(IOptions<ConfigurationOptions.JwtOptions> jwtOptions)
    {
        _jwtOptions = jwtOptions.Value;
    }

    public Token CreateAccessToken(User user, string refreshToken)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecurityKey));

        var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var expirationDate = DateTime.UtcNow.AddMinutes(TokenConstants.JwtTokenValidUntilMinutes);

        var securityToken = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: SetClaims(user),
            expires: expirationDate,
            notBefore: DateTime.UtcNow,
            signingCredentials: signingCredentials
        );

        var tokenHandler = new JwtSecurityTokenHandler();

        var tokenInstance = new Token(tokenHandler.WriteToken(securityToken), expirationDate, refreshToken);

        return tokenInstance;
    }

    private IEnumerable<Claim> SetClaims(User user)
    {
        var claims = new List<Claim>
        {
            new(TokenConstants.JwtClaimNames.UserId, user.Id.ToString()),
            new(TokenConstants.JwtClaimNames.Email, user.Email),
            new(TokenConstants.JwtClaimNames.UserType, user.UserType.ToString())
        };

        return claims;
    }
}
