using Microsoft.Extensions.Logging;
using SfmcApp.Models;

namespace SfmcApp.ViewModels;

public abstract class BaseSfmcViewModel<T> : BaseViewModel<T>
{
    public string ConnectionName => _sfmcConnection.Name;
    public string Title => $"Asset Navigator: Connected to {_sfmcConnection.Name}";

    private string _downloadDirectory;
    public string DownloadDirectory
    {
        get => _downloadDirectory;
        set => SetProperty(ref _downloadDirectory, value);
    }

    public BaseSfmcViewModel
    (
        ILogger<T> logger,
        SfmcConnection sfmcConnection,
        string resourceType
    )
        : base
        (
            logger: logger
        )
    {
        _downloadDirectory = Path.Combine(FileSystem.AppDataDirectory, "Downloads", sfmcConnection.DirectoryName, resourceType);
        _sfmcConnection = sfmcConnection;
    }

    protected readonly SfmcConnection _sfmcConnection;
}
