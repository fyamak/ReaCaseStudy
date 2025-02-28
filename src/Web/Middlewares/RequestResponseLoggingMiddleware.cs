using System.Text.Json;
using Serilog.Events;
using Shared.Extensions;
using ILogger = Serilog.ILogger;

namespace Web.Middlewares;

public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger         _logger;

    public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger logger)
    {
        _next   = next;
        _logger = logger.ForContext("SourceContext", GetType().FullName);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var requestTime = DateTime.UtcNow;

        string? requestBodyString = null;

        if (context.Request.ContentType?.StartsWith("application/json") ?? false)
        {
            context.Request.EnableBuffering();
            requestBodyString = await new StreamReader(context.Request.Body).ReadToEndAsync();
            context.Request.Body.Seek(0, SeekOrigin.Begin);
        }

        var headerDictionary = context.Request.Headers.ToDictionary(k => k.Key, v => string.Join(",", v));
        var requestHeaders   = JsonSerializer.Serialize(headerDictionary);

        var     originalResponseBodyStream = context.Response.Body;
        string? responseBodyString         = null;
        double  responseSec;

        using (var responseBody = new MemoryStream())
        {
            context.Response.Body = responseBody;

            await _next(context);

            responseSec = DateTime.UtcNow.Subtract(requestTime).TotalSeconds;

            if ((context.Response.ContentType?.StartsWith("application/json", StringComparison.OrdinalIgnoreCase) ?? false)
                || (context.Response.ContentType?.StartsWith("text/plain",    StringComparison.OrdinalIgnoreCase) ?? false))
            {
                responseBody.Seek(0, SeekOrigin.Begin);
                responseBodyString = await new StreamReader(responseBody).ReadToEndAsync();
            }

            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalResponseBodyStream);
        }


        _logger.LogExtended(LogEventLevel.Information,
            $"HTTP \"{context.Request.Method}\" to \"{context.Request.Path}\" Responded with {context.Response.StatusCode} in {responseSec} seconds",
            customData:
            new Dictionary<string, string?>
            {
                { "RequestPath", context.Request.Path },
                { "RequestQueryString", context.Request.QueryString.Value },
                { "RequestHeaders", requestHeaders },
                { "RequestBody", requestBodyString },
                { "ResponseBody", responseBodyString },
                { "StatusCode", context.Response.StatusCode.ToString() }
            }
        );
    }
}
