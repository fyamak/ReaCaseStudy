namespace Shared.Models.Configuration;

public abstract class ConfigurationOptions
{
    public record JwtOptions
    {
        public const string Jwt = "Jwt";

        public string SecurityKey { get; set; } = default!;
        public string Issuer      { get; set; } = default!;
        public string Audience    { get; set; } = default!;
    }

    public record SmtpOptions
    {
        public const string Smtp = "Smtp";

        public string Host     { get; set; } = default!;
        public string Email    { get; set; } = default!;
        public string Password { get; set; } = default!;
        public int    Port     { get; set; } = default!;
    }

    public record FrontAppOptions
    {
        public const string FrontApp = "FrontApp";

        public string Url { get; set; } = default!;
    }

    public record AppOptions
    {
        public const string App = "App";

        public string Name             { get; set; } = default!;
        public string EnvironmentAlias { get; set; } = default!;
    }
}
