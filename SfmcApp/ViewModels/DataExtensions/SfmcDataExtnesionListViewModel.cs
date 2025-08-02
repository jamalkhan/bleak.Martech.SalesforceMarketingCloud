using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Sfmc.Soap;
using bleak.Martech.SalesforceMarketingCloud.Sfmc.Rest.Assets;
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
            DataExtensionFolderSoapApi,
            AssetViewModel,
            DataExtensionSoapApi
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
        DataExtensionFolderSoapApi folderApi,
        DataExtensionSoapApi assetApi
    )
        : base
        (
            logger: logger,
            sfmcConnection: sfmcConnection,
            folderApi: folderApi,
            assetApi: assetApi,
            resourceType: "Assets"

        )
    {
        FolderTappedCommand = new Command<FolderViewModel>(folder => SelectedFolder = folder);
        SearchCommand = new Command(() => OnSearchButtonClicked());
        OpenDownloadDirectoryCommand = new Command(OpenDownloadDirectory);

        LoadFoldersAsync();
    }

    private void OnSearchButtonClicked()
    {
        // Search logic goes here
    }

    public ICommand DownloadCommand => new Command<AssetViewModel>(async asset =>
    {
        
    });

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

    public override Task LoadAssetForSelectedFolderAsync()
    {
        throw new NotImplementedException();
    }
}