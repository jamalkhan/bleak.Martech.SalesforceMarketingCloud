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

        LoadFoldersAsync();
    }

    public override async Task LoadFoldersAsync()
    {
        
        try
        {
            IsFoldersLoaded = false;
            IsFoldersLoading = true;
            var folderTree = await FolderApi.GetFolderTreeAsync();
            Folders.Clear();
            foreach (var folder in folderTree.ToViewModel())
            {
                Folders.Add(folder);
            }
            IsFoldersLoaded = true;
            IsFoldersLoading = false;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error loading folders. {ex.Message}");
        }
    }

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
            long records = await Task.Run(() =>
                RestApi.DownloadDataExtension(
                    dataExtensionCustomerKey: dataExtension.CustomerKey,
                    fileWriter: fileWriter,
                    fileName: filePath
                )
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
}