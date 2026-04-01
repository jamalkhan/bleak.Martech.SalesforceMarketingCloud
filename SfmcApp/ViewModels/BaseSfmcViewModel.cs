using Microsoft.Extensions.Logging;
using SfmcApp.Models;
using SfmcApp.ViewModels.Services;

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

    public void OpenDownloadDirectory()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(DownloadDirectory))
            {
                _logger.LogWarning("OpenDownloadDirectory was requested but no download directory is configured.");
                return;
            }

            _logger.LogInformation("Opening download directory {DownloadDirectory} for connection {ConnectionName}.", DownloadDirectory, ConnectionName);

#if WINDOWS
            var process = new System.Diagnostics.Process();
            process.StartInfo = new System.Diagnostics.ProcessStartInfo("explorer.exe", DownloadDirectory)
            {
                UseShellExecute = true
            };
            process.Start();
#elif MACCATALYST || MACOS
            var process = new System.Diagnostics.Process();
            process.StartInfo = new System.Diagnostics.ProcessStartInfo("open", $"\"{DownloadDirectory}\"")
            {
                UseShellExecute = true
            };
            process.Start();
#else
            // Optionally handle other platforms (Linux, Android, iOS)
#endif
        }
        catch (Exception ex)
        {
            // Optionally log or display an error
            _logger.LogError(ex, "Failed to open download directory {DownloadDirectory}.", DownloadDirectory);
        }
    }

    public BaseSfmcViewModel
    (
        INavigationService navigationService,
        ILogger<T> logger,
        SfmcConnection sfmcConnection,
        string resourceType
    )
        : base
        (
            navigationService: navigationService,
            logger: logger
        )
    {
        _downloadDirectory = Path.Combine(FileSystem.AppDataDirectory, "Downloads", sfmcConnection.DirectoryName, resourceType);
        _sfmcConnection = sfmcConnection;
        _logger.LogInformation("Initialized {ViewModelName} for connection {ConnectionName}. ResourceType={ResourceType}, DownloadDirectory={DownloadDirectory}", typeof(T).Name, sfmcConnection.Name, resourceType, _downloadDirectory);
        _logger.LogDebug("Connection state. Subdomain={Subdomain}, AuthBaseUrl={AuthBaseUrl}, RestBaseUrl={RestBaseUrl}, SoapBaseUrl={SoapBaseUrl}", sfmcConnection.Subdomain, sfmcConnection.AuthBaseUrl, sfmcConnection.RestBaseUrl, sfmcConnection.SoapBaseUrl);
    }

    protected readonly SfmcConnection _sfmcConnection;
}
