using Serilog;
using Serilog.Context;
using Serilog.Events;

namespace Shared.Extensions;

public static class LogExtensions
{
    public static void LogExtended(this ILogger logger, LogEventLevel level, string message, Exception? exception = null,
        Dictionary<string, string?>?            customData = null)
    {
        using (new LogContextScope(customData))
        {
            logger.Write(level, exception, message);
        }
    }


    private class LogContextScope : IDisposable
    {
        private readonly IList<IDisposable> _disposables;

        public LogContextScope(Dictionary<string, string?>? customData)
        {
            _disposables = new List<IDisposable>();

            if (customData != null)
            {
                foreach (KeyValuePair<string, string?> kv in customData)
                {
                    _disposables.Add(LogContext.PushProperty(kv.Key, kv.Value));
                }
            }
        }

        public void Dispose()
        {
            foreach (IDisposable disposable in _disposables)
            {
                disposable.Dispose();
            }
        }
    }
}
