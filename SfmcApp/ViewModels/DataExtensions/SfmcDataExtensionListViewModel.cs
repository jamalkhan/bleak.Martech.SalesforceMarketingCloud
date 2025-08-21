using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.Api;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Sfmc.Soap;
using bleak.Martech.SalesforceMarketingCloud.Fileops;
using bleak.Martech.SalesforceMarketingCloud.Sfmc.Rest.DataExtensions;
using Microsoft.Extensions.Logging;
using SfmcApp.Models;
using SfmcApp.Models.ViewModels;

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
        SfmcConnection sfmcConnection,
        ILogger<SfmcDataExtensionListViewModel> logger,
        IDataExtensionFolderApi folderApi,
        IDataExtensionApi contentResourceApi,
        DataExtensionRestApi deRestApi
    )
       : base
       (
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
    }

/*
    public async Task LoadFoldersAsync()
    {
        try
        {
            _logger.LogInformation("Loading Base folders...");
            IsFoldersLoaded = false;
            IsFoldersLoading = true;
            _logger.LogInformation("Set Booleans");
            _logger.LogInformation("Calling GetFolderTreeAsync...");
            _logger.LogInformation($"FolderAPI: {FolderApi != null} {FolderApi.GetType().Name}");
            var folderTree = await FolderApi.GetFolderTreeAsync();
            _logger.LogInformation("Set Booleans");
            Folders.Clear();
            _logger.LogInformation("Cleared Folders collection.");
            foreach (var folder in folderTree.ToViewModel())
            {
                _logger.LogInformation($"Adding folder: {folder.Name}");
                Folders.Add(folder);
            }
            _logger.LogInformation("Folders loaded successfully.");
            _logger.LogInformation($"Total folders loaded: {Folders.Count}");
            IsFoldersLoaded = true;
            IsFoldersLoading = false;
            _logger.LogInformation("Set Booleans");
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
            var contentResources = await PerformSearchAsync();
            PopulateContentResources(contentResources);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to perform search: {ex.Message}");
        }
    }

    private async Task<List<DataExtensionViewModel>> PerformSearchAsync()
    {
        SelectedFolder = new FolderViewModel() { Name = $"Search for %{SearchText}% | No Folder Selected" };
        string searchType = SelectedSearchOption.ToString() ?? "Like";

        // Check if search text is empty
        if (string.IsNullOrEmpty(searchType))
        {
            _logger.LogError("Please enter a search term.");
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

        return contentResources.ToViewModel();
    }
    #endregion Search

    public ICommand DownloadCommand => new Command<DataExtensionViewModel>(async dataExtension =>
    {

        var filePath = Path.Combine(FileSystem.AppDataDirectory, $"{dataExtension.Name}.csv");

        try
        {
            // Optional: Show loading UI
            _logger.LogInformation($"Starting download of {dataExtension.Name} to {filePath}...", "OK");


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

            _logger.LogInformation($"Downloaded {records} records to {filePath}", "OK");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Download failed: {ex.ToString()}", "OK");
        }

    });

    public async override Task LoadContentResourcesForSelectedFolderAsync()
    {
        if (SelectedFolder == null) return;
        try
        {
            var contentResources = await ContentResourceApi.GetDataExtensionsByFolderAsync(SelectedFolder.Id);
            PopulateContentResources(contentResources.ToViewModel());
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to load assets. {ex.Message}");
        }
    }

    public async override Task<IEnumerable<FolderViewModel>> GetFolderTreeAsync()
    {
         try
        {
            _logger.LogInformation("Calling FolderApi.GetFolderTreeAsync()");
            
            // Add timeout to prevent infinite waiting
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var folders = await FolderApi.GetFolderTreeAsync().WaitAsync(cts.Token);
            
            _logger.LogInformation($"Received {folders?.Count() ?? 0} folders from API");
            var viewModels = folders?.ToViewModel();
            _logger.LogInformation($"Converted to {viewModels?.Count() ?? 0} view models");
            return viewModels ?? Enumerable.Empty<FolderViewModel>();
        }
        catch (OperationCanceledException)
        {
            _logger.LogError("API call timed out after 30 seconds");
            throw new TimeoutException("API call timed out. Please check your connection and try again.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting folder tree from API");
            throw;
        }
    }
}