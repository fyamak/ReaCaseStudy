using System.Diagnostics;
using System.Net;
using Microsoft.Extensions.Options;
using Shared.Models;
using Shared.Models.Configuration;

namespace Web.Middlewares;

public class Rea404Middleware
{
    private readonly RequestDelegate                 _next;
    private readonly ConfigurationOptions.AppOptions _appConfig;

    public Rea404Middleware(RequestDelegate next, IOptions<ConfigurationOptions.AppOptions> appConfig)
    {
        _next      = next;
        _appConfig = appConfig.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);

        if (context.Response.StatusCode == (int)HttpStatusCode.NotFound)
        {
            var response = context.Response;

            var assembly = typeof(WebModule).Assembly;

            var creationDate = File.GetCreationTime(assembly.Location);

            var version = FileVersionInfo.GetVersionInfo(assembly.Location).ProductVersion;

            const string signature = """
                                               _____                    _____                    _____
                                              /\    \                  /\    \                  /\    \
                                             /::\    \                /::\    \                /::\    \
                                            /::::\    \              /::::\    \              /::::\    \
                                           /::::::\    \            /::::::\    \            /::::::\    \
                                          /:::/\:::\    \          /:::/\:::\    \          /:::/\:::\    \
                                         /:::/__\:::\    \        /:::/__\:::\    \        /:::/__\:::\    \
                                        /::::\   \:::\    \      /::::\   \:::\    \      /::::\   \:::\    \
                                       /::::::\   \:::\    \    /::::::\   \:::\    \    /::::::\   \:::\    \
                                      /:::/\:::\   \:::\____\  /:::/\:::\   \:::\    \  /:::/\:::\   \:::\    \
                                     /:::/  \:::\   \:::|    |/:::/__\:::\   \:::\____\/:::/  \:::\   \:::\____\
                                     \::/   |::::\  /:::|____|\:::\   \:::\   \::/    /\::/    \:::\  /:::/    /
                                      \/____|:::::\/:::/    /  \:::\   \:::\   \/____/  \/____/ \:::\/:::/    /
                                            |:::::::::/    /    \:::\   \:::\    \               \::::::/    /
                                            |::|\::::/    /      \:::\   \:::\____\               \::::/    /
                                            |::| \::/____/        \:::\   \::/    /               /:::/    /
                                            |::|  ~|               \:::\   \/____/               /:::/    /
                                            |::|   |                \:::\    \                  /:::/    /
                                            \::|   |                 \:::\____\                /:::/    /
                                             \:|   |                  \::/    /                \::/    /
                                              \|___|                   \/____/                  \/____/

                                     """;

            await response.WriteAsync(
                $"{signature}\n\n Project: {_appConfig.Name}, Environment: {_appConfig.EnvironmentAlias}, Version: {version}, Last Updated: {creationDate:dd.MM.yyyy}");
        }
    }
}
