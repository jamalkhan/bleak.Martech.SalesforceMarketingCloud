using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.Api;
using bleak.Martech.SalesforceMarketingCloud.Models.Pocos;
using bleak.Martech.SalesforceMarketingCloud.Sfmc.Rest.Assets;
using Microsoft.Extensions.Logging;
using SfmcApp.Models;
using SfmcApp.Models.ViewModels;
using SfmcApp.ViewModels.Services;

namespace SfmcApp.ViewModels;

public partial class SfmcAssetListViewModel
    : BaseSfmcFolderAndListViewModel
        <
            SfmcAssetListViewModel,
            FolderViewModel,
            IAssetFolderRestApi,
            AssetViewModel,
            IAssetRestApi
        >
    , INotifyPropertyChanged
{
    public ICommand FolderTappedCommand { get; }
    public ICommand SearchCommand { get; }
    public ICommand OpenDownloadDirectoryCommand { get; }

     public SfmcAssetListViewModel
    (
        INavigationService navigationService,
        SfmcConnection sfmcConnection,
        ILogger<SfmcAssetListViewModel> logger,
        IAssetFolderRestApi folderApi,
        IAssetRestApi contentResourceApi
    )
        : base
        (
            navigationService: navigationService,
            logger: logger,
            sfmcConnection: sfmcConnection,
            folderApi: folderApi,
            contentResourceApi: contentResourceApi,
            resourceType: "Assets"

        )
    {
        FolderTappedCommand = new Command<FolderViewModel>(folder => SelectedFolder = folder);
        SearchCommand = new Command(OnSearchButtonClicked);
        OpenDownloadDirectoryCommand = new Command(OpenDownloadDirectory);
    }

    public override async Task<IEnumerable<FolderViewModel>> GetFolderTreeAsync()
    {
        try
        {
            _logger.LogDebug("Requesting asset folder tree from API.");
            
            // Add timeout to prevent infinite waiting
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var folders = await FolderApi.GetFolderTreeAsync().WaitAsync(cts.Token);
            
            _logger.LogInformation("Received {FolderCount} asset folders from API.", folders?.Count() ?? 0);
            var viewModels = folders?.ToViewModel();
            _logger.LogDebug("Converted asset folders to {ViewModelCount} view models.", viewModels?.Count() ?? 0);
            return viewModels ?? Enumerable.Empty<FolderViewModel>();
        }
        catch (OperationCanceledException)
        {
            _logger.LogError("Asset folder API call timed out after 30 seconds.");
            throw new TimeoutException("API call timed out. Please check your connection and try again.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting folder tree from API");
            throw;
        }
    }
    

    private bool _expandAmpscript = true;
    public bool ExpandAmpscript
    {
        get => _expandAmpscript;
        set => SetProperty(ref _expandAmpscript, value);
    }

    
    public override async Task LoadContentResourcesForSelectedFolderAsync()
    {
        if (SelectedFolder == null) return;

        try
        {
            _logger.LogInformation("Loading assets for selected folder {FolderName} ({FolderId}).", SelectedFolder.Name, SelectedFolder.Id);
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
                _logger.LogTrace($"Added asset: {asset.Name} ({asset.AssetType.Name}) Count {ContentResources.Count}");
            }
            IsContentResourcesLoading = false;
            IsContentResourcesLoaded = true;
            _logger.LogInformation("Loaded {AssetCount} assets for folder {FolderName}.", ContentResources.Count, SelectedFolder.Name);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load assets for folder {FolderName} ({FolderId}).", SelectedFolder.Name, SelectedFolder.Id);
        }
    }

    private async void OnSearchButtonClicked()
    {
        try
        {
            _logger.LogInformation("Executing asset search. SearchText={SearchText}, SearchOption={SearchOption}", SearchText, SelectedSearchOption);
            var contentResources = await PerformSearchAsync();
            PopulateContentResources(contentResources);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to perform asset search. SearchText={SearchText}", SearchText);
        }
    }

    private async Task<List<AssetViewModel>> PerformSearchAsync()
    {
        SelectedFolder = new FolderViewModel() { Name = $"Search for %{SearchText}% | No Folder Selected" };
        string searchType = SelectedSearchOption.ToString() ?? "Like";

        // Check if search text is empty
        if (string.IsNullOrEmpty(searchType))
        {
            _logger.LogWarning("Asset search requested without a search term.");
            searchType = "Like";
        }

        var contentResources = new List<AssetPoco>();
        contentResources.AddRange(await ContentResourceApi.SearchAssetsAsync(SearchText));
        _logger.LogInformation("Asset search completed. SearchText={SearchText}, ResultCount={ResultCount}", SearchText, contentResources.Count);
        //throw new NotImplementedException($"Search type '{searchType}' is not implemented.");

        /*
        switch (searchType)
        {
            
            case "Starts With":
                contentResources = await ContentResourceApi.GetAssetsLikeAsync(SearchText);
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
        */
        return contentResources.ToViewModel();
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

        _logger.LogDebug("Writing asset metadata file. AssetName={AssetName}, MetadataFile={MetadataFile}", asset.Name, metadataFile);
        await File.WriteAllTextAsync(metadataFile, serializer.Serialize(asset));
        string outputFileName = Path.Combine(DownloadDirectory, fileName);
        _logger.LogInformation("Writing text asset content. AssetName={AssetName}, OutputFile={OutputFile}, ExpandAmpscript={ExpandAmpscript}", asset.Name, outputFileName, ExpandAmpscript);

        if (ExpandAmpscript)
        {
            var expandedContent = await GetExpandedContentAsync(asset);
            //var expandedContent = await asset.GetExpandedContentAsync();
            _logger.LogDebug("Expanding ampscript for asset {AssetName}.", asset.Name);

            // Write the expanded content to a file
            await File.WriteAllTextAsync(outputFileName, expandedContent);
            _logger.LogInformation("Expanded asset content written. AssetName={AssetName}, OutputFile={OutputFile}", asset.Name, outputFileName);
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
            
            await File.WriteAllTextAsync(outputFileName, content);
            _logger.LogInformation("Asset content written. AssetName={AssetName}, OutputFile={OutputFile}, ContentLength={ContentLength}", asset.Name, outputFileName, content.Length);
        }
    }

    private async Task DownloadBinaryAsync(AssetViewModel asset)
    {
        JsonSerializer serializer = new JsonSerializer();
        string fileName = GetFileName(asset);

        // TODO: Make the file path include the connection name or some identifier
        var metadataFile = Path.Combine(DownloadDirectory, $"{fileName}.metadata.json");

        Directory.CreateDirectory(Path.GetDirectoryName(metadataFile)!);
        _logger.LogDebug("Writing binary asset metadata. AssetName={AssetName}, MetadataFile={MetadataFile}", asset.Name, metadataFile);

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
                _logger.LogInformation("Binary asset saved. AssetName={AssetName}, OutputFile={OutputFile}", asset.Name, imageFilePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to download binary asset. AssetName={AssetName}, Url={Url}", asset.Name, imageUrl);
            }
        }
    }        
}
