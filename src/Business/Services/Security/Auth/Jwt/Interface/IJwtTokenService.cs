using Business.Services.Security.Auth.Jwt.Models;
using Infrastructure.Data.Postgres.Entities;

namespace Business.Services.Security.Auth.Jwt.Interface;

public interface IJwtTokenService
{
    Token CreateAccessToken(User user, string refreshToken);
}
