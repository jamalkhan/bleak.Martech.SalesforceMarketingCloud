using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.Api;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Sfmc.Soap;
using bleak.Martech.SalesforceMarketingCloud.Fileops;
using bleak.Martech.SalesforceMarketingCloud.Models.Pocos;
using bleak.Martech.SalesforceMarketingCloud.Sfmc.Rest.DataExtensions;
using Microsoft.Extensions.Logging;
using SfmcApp.Models;
using SfmcApp.Models.ViewModels;
using SfmcApp.ViewModels.Services;

namespace SfmcApp.ViewModels;

public partial class SfmcDataExtensionListViewModel
    : BaseSfmcFolderAndListViewModel
        <
            SfmcDataExtensionListViewModel,
            FolderViewModel,
            IDataExtensionFolderApi,
            DataExtensionViewModel,
            IDataExtensionApi
        >
    , INotifyPropertyChanged
{
    public DataExtensionRestApi RestApi  { get; }

    public ICommand FolderTappedCommand { get; }
    public ICommand SearchCommand { get; }
    public ICommand OpenDownloadDirectoryCommand { get; }

    public SfmcDataExtensionListViewModel
    (
        INavigationService navigationService,
        SfmcConnection sfmcConnection,
        ILogger<SfmcDataExtensionListViewModel> logger,
        IDataExtensionFolderApi folderApi,
        IDataExtensionApi contentResourceApi,
        DataExtensionRestApi deRestApi
    )
       : base
       (
           navigationService: navigationService,
           logger: logger,
           sfmcConnection: sfmcConnection,
           folderApi: folderApi,
           contentResourceApi: contentResourceApi,
           resourceType: "DataExtensions"
       )
    {
        RestApi = deRestApi;

        FolderTappedCommand = new Command<FolderViewModel>(folder => SelectedFolder = folder);
        SearchCommand = new Command(OnSearchButtonClicked);
        OpenDownloadDirectoryCommand = new Command(OpenDownloadDirectory);
        FileDroppedCommand = new Command<List<string>>(OnFileDropped);

    }
    public ICommand FileDroppedCommand { get; }

    private async void OnFileDropped(List<string> filePaths)
    {
        if (filePaths.Count == 0)
            return;

        if (SelectedFolder == null)
        {
            _logger.LogWarning("Import requested without a selected folder.");
            return;
        }

        try
        {
            _logger.LogInformation("Opening import flow. FilePath={FilePath}, SelectedFolder={SelectedFolder}", filePaths[0], SelectedFolder.Name);
            await _navigationService.NavigateToFileImportAsync(_sfmcConnection, filePaths[0], SelectedFolder);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open import flow for file {FilePath}", filePaths[0]);
        }
    }

    public Task OpenImportFlowAsync(string filePath)
    {
        if (SelectedFolder == null)
            throw new InvalidOperationException("Select a target folder before importing.");

        return _navigationService.NavigateToFileImportAsync(_sfmcConnection, filePath, SelectedFolder);
    }

/*
            public async Task LoadFoldersAsync()
            {
                try
                {
                    _logger.LogTrace("Loading Base folders...");
                    IsFoldersLoaded = false;
                    IsFoldersLoading = true;
                    _logger.LogTrace("Set Booleans");
                    _logger.LogTrace("Calling GetFolderTreeAsync...");
                    _logger.LogTrace($"FolderAPI: {FolderApi != null} {FolderApi.GetType().Name}");
                    var folderTree = await FolderApi.GetFolderTreeAsync();
                    _logger.LogTrace("Set Booleans");
                    Folders.Clear();
                    _logger.LogTrace("Cleared Folders collection.");
                    foreach (var folder in folderTree.ToViewModel())
                    {
                        _logger.LogTrace($"Adding folder: {folder.Name}");
                        Folders.Add(folder);
                    }
                    _logger.LogTrace("Folders loaded successfully.");
                    _logger.LogTrace($"Total folders loaded: {Folders.Count}");
                    IsFoldersLoaded = true;
                    IsFoldersLoading = false;
                    _logger.LogTrace("Set Booleans");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"View Model: Error loading Data Extension folders. {Environment.NewLine}Stack Trace ----------{Environment.NewLine} {ex.StackTrace}");
                }
            }
            */

    #region Search

    private async void OnSearchButtonClicked()
    {
        try
        {
            _logger.LogInformation("Executing data extension search. SearchText={SearchText}, SearchOption={SearchOption}", SearchText, SelectedSearchOption);
            var contentResources = await PerformSearchAsync();
            PopulateContentResources(contentResources);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to perform data extension search. SearchText={SearchText}", SearchText);
        }
    }

    private async Task<List<DataExtensionViewModel>> PerformSearchAsync()
    {
        SelectedFolder = new FolderViewModel() { Name = $"Search for %{SearchText}% | No Folder Selected" };
        string searchType = SelectedSearchOption.ToString() ?? "Like";

        // Check if search text is empty
        if (string.IsNullOrEmpty(searchType))
        {
            _logger.LogWarning("Data extension search requested without a search term.");
            searchType = "Like";
        }

        var contentResources = new List<DataExtensionPoco>();
        switch (searchType)
        {
            case "Starts With":
                contentResources = await ContentResourceApi.GetDataExtensionsNameStartsWithAsync(SearchText);
                break;
            case "Like":
                contentResources = await ContentResourceApi.GetDataExtensionsNameLikeAsync(SearchText);
                break;
            case "Ends With":
                contentResources = await ContentResourceApi.GetDataExtensionsNameEndsWithAsync(SearchText);
                break;
            default:
                contentResources = await ContentResourceApi.GetDataExtensionsNameLikeAsync(SearchText);
                break;
        }

        _logger.LogInformation("Data extension search completed. SearchText={SearchText}, SearchType={SearchType}, ResultCount={ResultCount}", SearchText, searchType, contentResources.Count);
        return contentResources.ToViewModel();
    }
    #endregion Search

    public ICommand DownloadCommand => new Command<DataExtensionViewModel>(async dataExtension =>
    {

        var filePath = Path.Combine(FileSystem.AppDataDirectory, $"{dataExtension.Name}.csv");

        try
        {
            // Optional: Show loading UI
            _logger.LogInformation("Starting data extension download. DataExtension={DataExtensionName}, CustomerKey={CustomerKey}, FilePath={FilePath}", dataExtension.Name, dataExtension.CustomerKey, filePath);


            IFileWriter fileWriter = new DelimitedFileWriter
            (
                new DelimitedFileWriterOptions { Delimiter = "," }
            );

            // Run long sync task in background
            long records = await 
                RestApi.DownloadDataExtensionAsync(
                    dataExtensionCustomerKey: dataExtension.CustomerKey,
                    fileWriter: fileWriter,
                    fileName: filePath
                );

            _logger.LogInformation("Data extension download completed. DataExtension={DataExtensionName}, Records={RecordCount}, FilePath={FilePath}", dataExtension.Name, records, filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Data extension download failed. DataExtension={DataExtensionName}, CustomerKey={CustomerKey}", dataExtension.Name, dataExtension.CustomerKey);
        }

    });

    public async override Task LoadContentResourcesForSelectedFolderAsync()
    {
        if (SelectedFolder == null) return;
        try
        {
            _logger.LogInformation("Loading data extensions for selected folder {FolderName} ({FolderId}).", SelectedFolder.Name, SelectedFolder.Id);
            var contentResources = await ContentResourceApi.GetDataExtensionsByFolderAsync(SelectedFolder.Id);
            PopulateContentResources(contentResources.ToViewModel());
            _logger.LogInformation("Loaded {DataExtensionCount} data extensions for folder {FolderName}.", ContentResources.Count, SelectedFolder.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load data extensions for folder {FolderName} ({FolderId}).", SelectedFolder.Name, SelectedFolder.Id);
        }
    }

    public async override Task<IEnumerable<FolderViewModel>> GetFolderTreeAsync()
    {
         try
        {
            _logger.LogDebug("Requesting data extension folder tree from API.");
            
            // Add timeout to prevent infinite waiting
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var folders = await FolderApi.GetFolderTreeAsync().WaitAsync(cts.Token);
            
            _logger.LogInformation("Received {FolderCount} data extension folders from API.", folders?.Count() ?? 0);
            var viewModels = folders?.ToViewModel();
            _logger.LogDebug("Converted data extension folders to {ViewModelCount} view models.", viewModels?.Count() ?? 0);
            return viewModels ?? Enumerable.Empty<FolderViewModel>();
        }
        catch (OperationCanceledException)
        {
            _logger.LogError("Data extension folder API call timed out after 30 seconds.");
            throw new TimeoutException("API call timed out. Please check your connection and try again.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting folder tree from API");
            throw;
        }
    }
}
