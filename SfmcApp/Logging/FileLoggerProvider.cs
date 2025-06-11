using Microsoft.Extensions.Logging;

namespace SfmcApp.Logging;

public class FileLoggerProvider : ILoggerProvider
{
    private readonly string _filePath;

    public FileLoggerProvider(string filePath)
    {
        _filePath = filePath;
    }

    public ILogger CreateLogger(string categoryName)
        => new FileLogger(categoryName, _filePath);

    public void Dispose() { }
}