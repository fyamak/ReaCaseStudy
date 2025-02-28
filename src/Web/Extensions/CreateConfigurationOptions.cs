using Shared.Models;
using Shared.Models.Configuration;

namespace Web.Extensions;

public static class CreateConfigurationOptions
{
    public static void CreateOptions(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.Configure<ConfigurationOptions.JwtOptions>(
            configuration.GetSection(ConfigurationOptions.JwtOptions.Jwt));

        serviceCollection.Configure<ConfigurationOptions.SmtpOptions>(
            configuration.GetSection(ConfigurationOptions.SmtpOptions.Smtp));

        serviceCollection.Configure<ConfigurationOptions.FrontAppOptions>(
            configuration.GetSection(ConfigurationOptions.FrontAppOptions.FrontApp));

        serviceCollection.Configure<ConfigurationOptions.AppOptions>(
            configuration.GetSection(ConfigurationOptions.AppOptions.App));
    }
}
