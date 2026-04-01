using Microsoft.Extensions.Logging;

namespace bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Logging;

public sealed class FileLogger : ILogger
{
    private readonly string _category;
    private readonly string _filePath;
    private static readonly object SyncLock = new();

    public FileLogger(string category, string filePath)
    {
        _category = category;
        _filePath = filePath;
    }

    public IDisposable BeginScope<TState>(TState state) where TState : notnull
    {
        return NullScope.Instance;
    }

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        var message = formatter(state, exception);
        var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{logLevel}] {_category}: {message}{Environment.NewLine}";

        lock (SyncLock)
        {
            File.AppendAllText(_filePath, line);
            if (exception is not null)
            {
                File.AppendAllText(_filePath, exception + Environment.NewLine);
            }
        }
    }

    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();
        public void Dispose() { }
    }
}
