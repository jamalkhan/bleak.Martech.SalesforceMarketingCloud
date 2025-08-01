using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.Api;
using bleak.Martech.SalesforceMarketingCloud.Sfmc.Rest.Assets;
using Microsoft.Extensions.Logging;
using SfmcApp.Models;
using SfmcApp.Models.ViewModels;

namespace SfmcApp.ViewModels;

public partial class SfmcAssetListViewModel
    : BaseSfmcFolderAndListViewModel<SfmcAssetListViewModel, FolderViewModel, IAssetFolderRestApi, AssetViewModel, IAssetRestApi>
    , INotifyPropertyChanged
{
    private bool _expandAmpscript = true;
    public bool ExpandAmpscript
    {
        get => _expandAmpscript;
        set => SetProperty(ref _expandAmpscript, value);
    }



    public ICommand FolderTappedCommand { get; }
    public ICommand SearchCommand { get; }
    public ICommand OpenDownloadDirectoryCommand { get; }

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
    public ObservableCollection<StringSearchOptions> SearchOptions { get; } =
        new(Enum.GetValues(typeof(StringSearchOptions)).Cast<StringSearchOptions>());

    private StringSearchOptions _selectedSearchOption = StringSearchOptions.Like;
    public StringSearchOptions SelectedSearchOption
    {
        get => _selectedSearchOption;
        set => SetProperty(ref _selectedSearchOption, value);
    }
    #endregion Search

    public SfmcAssetListViewModel
    (
        SfmcConnection sfmcConnection,
        ILogger<SfmcAssetListViewModel> logger,
        IAssetFolderRestApi folderApi,
        IAssetRestApi assetApi
    )
        : base
        (
            logger: logger,
            sfmcConnection: sfmcConnection,
            folderApi: folderApi,
            contentResourceApi: assetApi,
            resourceType: "Assets"

        )
    {
        FolderTappedCommand = new Command<FolderViewModel>(folder => SelectedFolder = folder);
        SearchCommand = new Command(() => OnSearchButtonClicked());
        OpenDownloadDirectoryCommand = new Command(OpenDownloadDirectory);

        LoadFoldersAsync();
    }

    



    public override async Task LoadContentResourcesForSelectedFolderAsync()
    {
        if (SelectedFolder == null) return;

        try
        {
            IsContentResourcesLoaded = false;
            IsContentResourcesLoading = true;
            ContentResources.Clear();
            var assets = await ContentResourceApi.GetAssetsAsync(SelectedFolder.Id);
            foreach (var asset in assets.ToViewModel())
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
            IsContentResourcesLoading = false;
            IsContentResourcesLoaded = true;

        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to load assets. {ex.Message}");
        }
    }

    private void OnSearchButtonClicked()
    {
        // Search logic goes here
    }

    public ICommand DownloadCommand => new Command<AssetViewModel>(async asset =>
    {
        if (asset.IsBinaryDownloadable)
        {
            await DownloadBinaryAsync(asset);
        }
        else
        {
            await DownloadTextAsync(asset);
        }
    });

    // TODO: Make this a configuration option
    bool usePublishedUrlName = true;

    private string GetFileName(AssetViewModel asset)
    {
        if (usePublishedUrlName)
        {
            string url = asset.FileProperties.PublishedURL;
            Uri uri = new Uri(url);
            string fileName = Path.GetFileName(uri.LocalPath);
            return fileName;
        }
        return asset.FileProperties.FileName;
    }
    private string GetTextFileName(AssetViewModel asset)
    {
        return $"{asset.Name}-{asset.CustomerKey}.{asset.AssetType.Name}.ampscript";
    }
    private async Task DownloadTextAsync(AssetViewModel asset)
    {
        var serializer = new JsonSerializer();
        var fileName = GetTextFileName(asset);
        var metadataFile = Path.Combine(DownloadDirectory, $"{fileName}.metadata.json");
        Directory.CreateDirectory(Path.GetDirectoryName(metadataFile)!);
        // Write Metadata to Named as Customer Key
        //var metaDataCustomerKey = AppConfiguration.Instance.OutputFolder + "/" + asset.FullPath + "/customerkey-" + asset.CustomerKey + ".metadata";

        _logger.LogTrace($"Writing {fileName} metadata file to {metadataFile}");
        await File.WriteAllTextAsync(metadataFile, serializer.Serialize(asset));
        string outputFileName = Path.Combine(DownloadDirectory, fileName);
        _logger.LogTrace($"Writing {fileName} {asset.AssetType.Name} content to {outputFileName}");

        if (ExpandAmpscript)
        {
            var expandedContent = await GetExpandedContentAsync(asset);
            //var expandedContent = await asset.GetExpandedContentAsync();
            _logger.LogInformation($"Expanding Ampscript for asset: {asset.Name}");

            // Write the expanded content to a file
            await File.WriteAllTextAsync(outputFileName, expandedContent);
            _logger.LogInformation($"Asset with Expanded content written to File System: {outputFileName}");
        }
        else
        {
            string content = string.Empty;
            // Write the content of the asset to a file
            if (!string.IsNullOrEmpty(asset.Content))
            {
                content = asset.Content;
            }
            else if (asset.Views?.Html?.Content != null)
            {
                content = asset.Views.Html.Content;
            }
            
            _logger.LogInformation($"Content: {content}");
            _logger.LogInformation($"Views: {asset.Views}");
            _logger.LogInformation($"Html: {asset.Views?.Html}");
            _logger.LogInformation($"Html.Content: {asset.Views?.Html?.Content}");
            await File.WriteAllTextAsync(outputFileName, content);
            _logger.LogInformation($"Asset written to File System: {outputFileName}");
        }
    }

    private async Task DownloadBinaryAsync(AssetViewModel asset)
    {
        JsonSerializer serializer = new JsonSerializer();
        string fileName = GetFileName(asset);

        // TODO: Make the file path include the connection name or some identifier
        var metadataFile = Path.Combine(DownloadDirectory, $"{fileName}.metadata.json");

        Directory.CreateDirectory(Path.GetDirectoryName(metadataFile)!);
        _logger.LogTrace($"Trying to save {metadataFile}");

        File.WriteAllText(metadataFile, serializer.Serialize(asset));

        var imageUrl = asset.FileProperties.PublishedURL;
        using (var client = new HttpClient())
        {
            try
            {
                var imageFilePath = Path.Combine(DownloadDirectory, fileName);
                var imageBytes = client.GetByteArrayAsync(imageUrl).Result;
                Directory.CreateDirectory(Path.GetDirectoryName(imageFilePath)!);
                await File.WriteAllBytesAsync(imageFilePath, imageBytes);
                _logger.LogInformation($"Image saved to {imageFilePath}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to download image from {imageUrl}: {ex.Message}");
            }
        }
    }        
}
