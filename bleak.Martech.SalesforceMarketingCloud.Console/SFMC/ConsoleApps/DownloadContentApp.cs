using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.Authentication;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Configuration;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Sfmc.Soap;
using bleak.Martech.SalesforceMarketingCloud.Models;
using bleak.Martech.SalesforceMarketingCloud.Models.Helpers;
using bleak.Martech.SalesforceMarketingCloud.Models.Sfmc;
using Microsoft.Extensions.Logging;

namespace bleak.Martech.SalesforceMarketingCloud.ConsoleApp.ConsoleApps
{

    public class DownloadContentApp : IConsoleApp
    {
        static JsonSerializer serializer = new JsonSerializer();
        private static int assetCounter = 0;
        private static int folderCounter = 0;
        private static HashSet<string> assetTypes = new HashSet<string>();
        IRestClientAsync _restClientAsync;
        IAuthRepository _authRepository;
        private readonly ILogger<DownloadContentApp> _logger;
        public DownloadContentApp(IRestClientAsync restClientAsync, IAuthRepository authRepository, ILogger<DownloadContentApp> logger)
        {
            _restClientAsync = restClientAsync;
            _authRepository = authRepository;
            _logger = logger;
        }

        public async Task Execute()
        {
            _logger.LogInformation("Loading content folder tree.");
            LoadFolders loadFolders = new LoadFolders(restClientAsync: _restClientAsync, authRepository: _authRepository, logger: _logger);
            var folderTree = await loadFolders.GetFolderTreeAsync();
            _logger.LogInformation("Content folder tree loaded. RootFolderCount={RootFolderCount}", folderTree.Count);
            // PrintChildren(folderTree);
            // PrintFolders(folderTree);
            DownloadAllAssets(folderTree);
        }

        private void DownloadAllAssets(List<FolderObject> folderTree)
        {
            _logger.LogInformation("Starting asset download for {FolderCount} root folders.", folderTree.Count);

            var path = AppConfiguration.Instance.OutputFolder;
            _logger.LogDebug("Ensuring output directory exists. Path={Path}", path);
            Directory.CreateDirectory(path);
            _logger.LogDebug("Output directory ready. Path={Path}", path);

            foreach (var folder in folderTree)
            {
                assetCounter = DownloadAllAssetsForAFolder(folder);
                folderCounter++;
                _logger.LogInformation("Processed folder {FolderNumber}. CurrentAssetCount={AssetCount}", folderCounter, assetCounter);
                
            }
            _logger.LogInformation("Asset download complete. TotalFolders={FolderCount}, TotalAssets={AssetCount}", folderCounter, assetCounter);

            _logger.LogInformation("Asset types discovered: {AssetTypes}", string.Join(", ", assetTypes.OrderBy(x => x)));
            foreach (var assetType in assetTypes)
            {
                _logger.LogDebug("Discovered asset type {AssetType}.", assetType);
            }
            
        }

        private int DownloadAllAssetsForAFolder(FolderObject folder)
        {
            foreach (var asset in folder.Assets)
            {
                assetTypes.Add(asset.AssetType.Name);

                // Write Metadata to Named as Customer Key
                var metaDataCustomerKey = AppConfiguration.Instance.OutputFolder + "/" + asset.FullPath + "/customerkey-" + asset.CustomerKey + ".metadata";
                Directory.CreateDirectory(Path.GetDirectoryName(metaDataCustomerKey)!);
                _logger.LogDebug("Writing asset metadata by customer key. Path={Path}, AssetId={AssetId}, CustomerKey={CustomerKey}", metaDataCustomerKey, asset.Id, asset.CustomerKey);
                File.WriteAllText(metaDataCustomerKey, serializer.Serialize(asset));

                // Write Metadata to Named as Id
                var metaDataId = AppConfiguration.Instance.OutputFolder + "/" + asset.FullPath + "/id-" + asset.Id + ".metadata";
                Directory.CreateDirectory(Path.GetDirectoryName(metaDataId)!);
                _logger.LogDebug("Writing asset metadata by id. Path={Path}, AssetId={AssetId}", metaDataId, asset.Id);
                File.WriteAllText(metaDataId, serializer.Serialize(asset));

                string outputFileName = string.Empty;
                switch (asset.AssetType.Name)
                {
                    case "codesnippetblock":
                        outputFileName = AppConfiguration.Instance.OutputFolder + "/" + asset.FullPath + "/customerkey-" + asset.CustomerKey + ".ampscript.html";
                        _logger.LogDebug("Writing asset content file. Path={Path}, AssetId={AssetId}, AssetType={AssetType}", outputFileName, asset.Id, asset.AssetType.Name);
                        File.WriteAllText(outputFileName, asset.Content);

                        outputFileName = AppConfiguration.Instance.OutputFolder + "/" + asset.FullPath + "/id-" + asset.Id + ".ampscript.html";
                        _logger.LogDebug("Writing asset content file. Path={Path}, AssetId={AssetId}, AssetType={AssetType}", outputFileName, asset.Id, asset.AssetType.Name);
                        File.WriteAllText(outputFileName, asset.Content);
                        break;
                    case "webpage":
                    case "htmlemail":
                    case "htmlblock":
                    case "templatebasedemail":
                        outputFileName = AppConfiguration.Instance.OutputFolder + "/" + asset.FullPath + "/customerkey-" + asset.CustomerKey + ".ampscript.html";
                        _logger.LogDebug("Writing asset HTML file. Path={Path}, AssetId={AssetId}, AssetType={AssetType}", outputFileName, asset.Id, asset.AssetType.Name);
                        File.WriteAllText(outputFileName, asset.Views.Html.Content);

                        outputFileName = AppConfiguration.Instance.OutputFolder + "/" + asset.FullPath + "/id-" + asset.Id + ".ampscript.html";
                        _logger.LogDebug("Writing asset HTML file. Path={Path}, AssetId={AssetId}, AssetType={AssetType}", outputFileName, asset.Id, asset.AssetType.Name);
                        File.WriteAllText(outputFileName, asset.Views.Html.Content);
                        break;
                    default:
                        break;
                }

                assetCounter++;
                if (assetCounter % AppConfiguration.Instance.PageSize == 0)
                {
                    _logger.LogInformation("Wrote {AssetCount} assets to the filesystem so far.", assetCounter);
                }
            }
            if (folder.SubFolders != null && folder.SubFolders.Count > 0)
            {
                foreach (var subfolder in folder.SubFolders)
                {
                    DownloadAllAssetsForAFolder(subfolder);
                }
            }

            return assetCounter;
        }

        private static void PrintChildren(List<FolderObject> folderTree)
        {
            Console.WriteLine("Print Children");
            Console.WriteLine("---------------------");
            foreach (var folder in folderTree)
            {
                folder.PrintChildren();
            }
            Console.WriteLine("---------------------");
        }

        private static void PrintFolders(List<FolderObject> folderTree)
        {
            Console.WriteLine("Print Folder Structure");
            Console.WriteLine("---------------------");
            foreach (var folder in folderTree)
            {
                folder.PrintStructure();
            }
            Console.WriteLine("---------------------");
        }

        
        
        private async Task GetAssetsByFolder(FolderObject folderObject)
        {
            int page = 1;
            int currentPageSize = 0;
            
            var sfmcAssets = new List<SfmcAsset>();
            do
            {
                try
                {
                    var token = await _authRepository.GetTokenAsync();
                    // Console.WriteLine($"Token: {token.access_token}");
                    _logger.LogDebug("Loading asset page {PageNumber} for folder {FolderPath}.", page, folderObject.FullPath);
                    //string url = $"https://{AppConfiguration.Instance.Subdomain}.rest.marketingcloudapis.com/asset/v1/content/assets?$page=1&$pagesize=100&$orderBy=name asc&$filter=category.id=5843
                    string uri = $"{bleak.Martech.SalesforceMarketingCloud.Configuration.SfmcEndpointUrls.GetRestEndpoint(AppConfiguration.Instance.Subdomain, "/asset/v1/content/assets", AppConfiguration.Instance.RestBaseUrl)}?$page={page}&$pagesize={AppConfiguration.Instance.PageSize}&$orderBy=name&$filter=category.id eq {folderObject.Id}";

                    _logger.LogTrace("Requesting assets for folder {FolderPath}. Uri={Uri}", folderObject.FullPath, uri);

                    var results = await _restClientAsync.ExecuteRestMethodAsync<SfmcRestWrapper<SfmcAsset>, string>(
                        uri: new Uri(uri),
                        verb: HttpVerbs.GET,
                        headers:
                            new List<Header>()
                            { 
                                new Header() { Name = "Content-Type", Value = "application/json" } ,
                                new Header() { Name = "Authorization", Value = $"Bearer {token.access_token}" }
                            }
                        );

                    if (results != null && results.UnhandledError != null && results.UnhandledError.Contains("401"))
                    {
                        _logger.LogWarning("Asset request returned unauthenticated response for folder {FolderPath}. Error={Error}", folderObject.FullPath, results.UnhandledError);
                        token = await _authRepository.GetTokenAsync();

                        results = await _restClientAsync.ExecuteRestMethodAsync<SfmcRestWrapper<SfmcAsset>, string>(
                            uri: new Uri(uri),
                            verb: HttpVerbs.GET,
                            headers:
                                new List<Header>()
                                { 
                                    new Header() { Name = "Content-Type", Value = "application/json" } ,
                                    new Header() { Name = "Authorization", Value = $"Bearer {token.access_token}" }
                                }
                            );
                    }

                    _logger.LogTrace("Asset request completed for folder {FolderPath}. HasResults={HasResults}", folderObject.FullPath, results?.Results != null);
                    if (results?.Error != null) _logger.LogWarning("Asset request returned an error for folder {FolderPath}. Error={Error}", folderObject.FullPath, results.Error);
                    
                    currentPageSize = results!.Results.items.Count();
                    sfmcAssets.AddRange(results.Results.items);
                    _logger.LogDebug("Loaded asset page {PageNumber} for folder {FolderPath}. PageCount={PageCount}, AggregateCount={AggregateCount}", page, folderObject.FullPath, currentPageSize, sfmcAssets.Count);

                    if (AppConfiguration.Instance.PageSize == currentPageSize)
                    {
                        _logger.LogTrace("Additional asset pages remain for folder {FolderPath}.", folderObject.FullPath);
                    }

                }
                catch (System.Exception ex)
                {
                    _logger.LogError(ex, "Failed loading assets for folder {FolderPath}.", folderObject.FullPath);

                    break;
                }
                page++;
            }
            while (AppConfiguration.Instance.PageSize == currentPageSize);


            if (sfmcAssets.Any())
            {
                _logger.LogInformation("Loaded {AssetCount} assets for folder {FolderPath}.", sfmcAssets.Count, folderObject.FullPath);
                foreach (SfmcAsset sfmcAsset in sfmcAssets)
                {
                    var asset = sfmcAsset.ToPoco();
                    asset.FullPath = folderObject.FullPath;
                    folderObject.Assets.Add(asset);
                }
            }

//            throw new Exception("Error Loading Folders");

        }

        
        private async Task AddChildrenAsync(FolderObject parentFolder, List<SfmcFolder> sfmcFolders)
        {
            var sfmcFolders_w_MatchingParentId = sfmcFolders.Where(x => x.parentId == parentFolder.Id).ToList();
            if (sfmcFolders_w_MatchingParentId.Any())
            {
                parentFolder.SubFolders = new List<FolderObject>();
                foreach (var sfmcFolder in sfmcFolders_w_MatchingParentId)
                {
                    var subfolder = sfmcFolder.ToFolderObject();
                    subfolder.FullPath = parentFolder.FullPath + subfolder.Name + "/";
                    await GetAssetsByFolder(subfolder);
                    await AddChildrenAsync(subfolder, sfmcFolders);
                    parentFolder.SubFolders.Add(subfolder);
                }
            }
        }
    }
}
