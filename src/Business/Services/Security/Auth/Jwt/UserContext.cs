using System.Security.Claims;
using Business.Services.Security.Auth.Jwt.Interface;
using Infrastructure.Data.Postgres.Entities;

namespace Business.Services.Security.Auth.Jwt;

public class UserContext : IUserContext
{
    public bool IsAuthenticated => _claims.Count > 0;

    private readonly Dictionary<string, string> _claims;

    public UserContext(Claim[] claims)
    {
        _claims = claims.ToDictionary(c => c.Type, c => c.Value);
    }

    public int GetUserId()
    {
        if (_claims.TryGetValue(TokenConstants.JwtClaimNames.UserId, out var userIdString) &&
            int.TryParse(userIdString, out var userId))
        {
            return userId;
        }

        throw new InvalidOperationException("UserId claim is missing or invalid.");
    }

    public UserType GetUserType()
    {
        if (_claims.TryGetValue(TokenConstants.JwtClaimNames.UserType, out var userTypeString) &&
            Enum.TryParse<UserType>(userTypeString, out var userType))
        {
            return userType;
        }

        throw new InvalidOperationException("Usertype claim is missing or invalid.");
    }

    public string GetClaimByType(string claimType)
    {
        if (_claims.TryGetValue(claimType, out var claimValue))
        {
            return claimValue;
        }

        throw new KeyNotFoundException($"Claim of type '{claimType}' was not found.");
    }
}
