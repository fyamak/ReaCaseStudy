using Infrastructure.Data.Postgres.Entities.Base;

namespace Infrastructure.Data.Postgres.Entities;

public class User : TrackedBaseEntity<int>
{
    public string   Email        { get; set; } = default!;
    public string   FullName     { get; set; } = default!;
    public byte[]   PasswordSalt { get; set; } = default!;
    public byte[]   PasswordHash { get; set; } = default!;
    public UserType UserType     { get; set; }
}

public enum UserType
{
    Admin = 0,
    User  = 10
}
