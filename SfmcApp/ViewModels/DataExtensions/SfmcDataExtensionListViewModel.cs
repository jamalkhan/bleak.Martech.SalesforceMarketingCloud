using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.Api;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Sfmc.Soap;
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
            AssetViewModel,
            IDataExtensionApi
        >
    , INotifyPropertyChanged
{
    public ICommand FolderTappedCommand { get; }
    public ICommand SearchCommand { get; }
    public ICommand OpenDownloadDirectoryCommand { get; }

    public SfmcDataExtensionListViewModel
    (
        SfmcConnection sfmcConnection,
        ILogger<SfmcDataExtensionListViewModel> logger,
        IDataExtensionFolderApi folderApi,
        IDataExtensionApi contentResourceApi
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
        FolderTappedCommand = new Command<FolderViewModel>(folder => SelectedFolder = folder);
        SearchCommand = new Command(() => OnSearchButtonClicked());
        OpenDownloadDirectoryCommand = new Command(OpenDownloadDirectory);

        LoadFoldersAsync();
    }

    public override async Task LoadFoldersAsync()
    {
        /*
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
        */
        throw new NotImplementedException();
    }

    private void OnSearchButtonClicked()
    {
        // Search logic goes here
    }

    public ICommand DownloadCommand => new Command<AssetViewModel>(async asset =>
    {

    });

    public async override Task LoadContentResourcesForSelectedFolderAsync()
    {
        if (SelectedFolder == null) return;

        try
        {
            IsContentResourcesLoaded = false;
            IsContentResourcesLoading = true;
            ContentResources.Clear();
            var contentResource = await ContentResourceApi.GetDataExtensionsByFolderAsync(SelectedFolder.Id);
            /*foreach (var asset in contentResource.ToViewModel())
            {
                try
                {
                    ContentResources.Add(asset);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error processing asset {asset.Name}.");
                    continue;
                }
                _logger.LogInformation($"Added asset: {asset.Name} ({asset.AssetType.Name}) Count {ContentResources.Count}");
            }
            */
            IsContentResourcesLoading = false;
            IsContentResourcesLoaded = true;

        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to load assets. {ex.Message}");
        }
    }
}