using Infrastructure.Data.Postgres.Entities;

namespace Business.Services.Security.Auth.Jwt.Interface;

public interface IUserContext
{
    bool     IsAuthenticated { get; }
    int      GetUserId();
    UserType GetUserType();
    string   GetClaimByType(string claimType);
}
