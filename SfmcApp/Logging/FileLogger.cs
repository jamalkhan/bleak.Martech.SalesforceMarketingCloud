

using Microsoft.Extensions.Logging;

namespace SfmcApp.Logging;
public class FileLogger : ILogger
{
    private readonly string _filePath;
    private readonly string _category;

    public FileLogger(string category, string filePath)
    {
        _category = category;
        _filePath = filePath;
    }

    public IDisposable BeginScope<TState>(TState state) => null!;
    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId,
        TState state, Exception exception, Func<TState, Exception?, string> formatter)
    {
        var logRecord = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{logLevel}] {_category}: {formatter(state, exception)}{Environment.NewLine}";
        File.AppendAllText(_filePath, logRecord);
    }
}
