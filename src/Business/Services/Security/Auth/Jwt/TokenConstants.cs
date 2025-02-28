namespace Business.Services.Security.Auth.Jwt;

public static class TokenConstants
{
    public const int JwtTokenValidUntilMinutes        = 10;
    public const int RefreshTokenValidUntilDays       = 14;
    public const int ResetPasswordTokenValidUntilDays = 1;

    public static class JwtClaimNames
    {
        public const string Email           = "Email";
        public const string UserType        = "UserType";
        public const string UserId          = "UserId";
        public const string BusinessGroupId = "BusinessGroupId";
        public const string BusinessIds     = "BusinessIds";
    }
}
