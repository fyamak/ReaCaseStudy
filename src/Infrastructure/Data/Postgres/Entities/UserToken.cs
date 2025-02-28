using Shared.Models.Event;

namespace Infrastructure.Data.Postgres.Entities;

public class UserToken : EntityWithEvents
{
    public string    Token      { get; set; }
    public TokenType TokenType  { get; set; }
    public DateTime  Expiration { get; set; }
    public int       UserId     { get; set; }
    public User      User       { get; set; }

    public UserToken(TokenType tokenType, DateTime expiration, int userId)
    {
        Token      = Guid.NewGuid().ToString();
        TokenType  = tokenType;
        Expiration = expiration;
        UserId     = userId;
    }
}

public enum TokenType
{
    RefreshToken  = 1,
    ResetPassword = 2
}
