using Microsoft.Extensions.Logging;

namespace bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Logging;

public sealed class FileLoggerProvider : ILoggerProvider
{
    private readonly string _filePath;

    public FileLoggerProvider(string filePath)
    {
        _filePath = filePath;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new FileLogger(categoryName, _filePath);
    }

    public void Dispose() { }
}
