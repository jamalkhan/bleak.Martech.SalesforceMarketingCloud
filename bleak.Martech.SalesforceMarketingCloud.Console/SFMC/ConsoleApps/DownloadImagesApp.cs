using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Configuration;
using bleak.Martech.SalesforceMarketingCloud.Authentication;
using bleak.Martech.SalesforceMarketingCloud.Models;
using bleak.Martech.SalesforceMarketingCloud.Models.Sfmc;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Sfmc.Soap;
using System.Diagnostics;
using System;
using System.IO;
using bleak.Martech.SalesforceMarketingCloud.Wsdl;
using bleak.Martech.SalesforceMarketingCloud.Models.Pocos;
using bleak.Martech.SalesforceMarketingCloud.Models.Helpers;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace bleak.Martech.SalesforceMarketingCloud.ConsoleApp.ConsoleApps
{

    public class DownloadImagesApp : IConsoleApp
    {
        static JsonSerializer serializer = new JsonSerializer();
        private static int assetCounter = 0;
        private static HashSet<string> assetTypes = new HashSet<string>();
        IRestClientAsync _restClient;
        IAuthRepository _authRepository;
        private readonly ILogger<DownloadImagesApp> _logger;
        string fullPath = string.Empty;
        public DownloadImagesApp(IRestClientAsync restClient, IAuthRepository authRepository, ILogger<DownloadImagesApp> logger)
        {
            _restClient = restClient;
            _authRepository = authRepository;
            _logger = logger;
        }

        public async Task Execute()
        {
            Console.WriteLine("Give me the folder ID");
            var folder = Console.ReadLine();
            
            int.TryParse(folder, out int folderId);
            await DownloadAllAssetsAsync(folderId);
        }



        private void CreateFolder()
        {
            // Get the path to the user's desktop
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            _logger.LogDebug("Using desktop path for image export. DesktopPath={DesktopPath}", desktopPath);

            // Define the name of the new folder
            string newFolderName = "MyNewFolder";

            // Combine to get the full path
            fullPath = Path.Combine(desktopPath, newFolderName);
            _logger.LogInformation("Preparing image export folder. Path={Path}", fullPath);

            _logger.LogDebug("Ensuring image export directory exists. Path={Path}", fullPath);
            Directory.CreateDirectory(fullPath);
            _logger.LogDebug("Image export directory ready. Path={Path}", fullPath);
        }


        private async Task DownloadAllAssetsAsync(int folderId)
        {
            CreateFolder();

            _logger.LogInformation("Starting image download for folder {FolderId}.", folderId);

            assetCounter = await DownloadAllAssetsForAFolderAsync(folderId);

            
            _logger.LogInformation("Image download complete for folder {FolderId}. TotalAssets={AssetCount}", folderId, assetCounter);

            _logger.LogInformation("Image workflow discovered asset types: {AssetTypes}", string.Join(", ", assetTypes.OrderBy(x => x)));
            foreach (var assetType in assetTypes)
            {
                _logger.LogDebug("Discovered asset type {AssetType}.", assetType);
            }
            
        }

        private async Task<int> DownloadAllAssetsForAFolderAsync(int folderId)
        {
            var assets = await GetAssetsByFolderAsync(folderId);

            foreach (var asset in assets)
            {
                assetTypes.Add(asset.AssetType.Name);

                // Write Metadata to Named as Customer Key
                //var metaDataCustomerKey = AppConfiguration.Instance.OutputFolder + "/" + asset.FullPath + "/customerkey-" + asset.CustomerKey + ".metadata";
                //Directory.CreateDirectory(Path.GetDirectoryName(metaDataCustomerKey)!);

                //if (AppConfiguration.Instance.Debug) { Console.WriteLine($"Writing Metadata file to to save {metaDataCustomerKey}"); }
 //               File.WriteAllText(metaDataCustomerKey, serializer.Serialize(asset));
//
                // Write Metadata to Named as Id
            //    var metaDataId = AppConfiguration.Instance.OutputFolder + "/" + asset.FullPath + "/id-" + asset.Id + ".metadata";
              //  Directory.CreateDirectory(Path.GetDirectoryName(metaDataId)!);
               //if (AppConfiguration.Instance.Debug) { Console.WriteLine($"Writing Metadata file to to save {metaDataId}"); }
                //File.WriteAllText(metaDataId, serializer.Serialize(asset));

               // string outputFileName = string.Empty;
                switch (asset.AssetType.Name)
                {
                    case "jpg":
                    case "png":
                    case "gif":
                        string metadataFile = fullPath + "/" + asset.FullPath + "/" + asset.FileProperties.FileName + ".metadata.json";
                        Directory.CreateDirectory(Path.GetDirectoryName(metadataFile)!);
                        _logger.LogDebug("Writing image metadata file. Path={Path}, AssetId={AssetId}", metadataFile, asset.Id);
                        File.WriteAllText(metadataFile, serializer.Serialize(asset));

                        var imageUrl = asset.FileProperties.PublishedURL;
                        using (var client = new HttpClient())
                        {
                            try
                            {
                                string fileName = GetFileName(asset);

                                var imageBytes = client.GetByteArrayAsync(imageUrl).Result;
                                var imageFilePath = fullPath + "/" + asset.FullPath + "/" + fileName;
                                Directory.CreateDirectory(Path.GetDirectoryName(imageFilePath)!);
                                File.WriteAllBytes(imageFilePath, imageBytes);
                                _logger.LogInformation("Image saved. Path={Path}, SourceUrl={SourceUrl}", imageFilePath, imageUrl);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Failed to download image. SourceUrl={SourceUrl}", imageUrl);
                            }
                        }
                        break;
                    default:
                        break;
                }

                assetCounter++;
                if (assetCounter % AppConfiguration.Instance.PageSize == 0)
                {
                    _logger.LogInformation("Wrote {AssetCount} image assets to the filesystem so far.", assetCounter);
                }
            }

            return assetCounter;
        }


        enum FileNameFormat{
            OriginalFileName,
            PublishedURLName,
        }
        private FileNameFormat? _fileNameFormat = null; 
        private string GetFileName(AssetPoco asset)
        {
            AskFileNameFormat();

            if (_fileNameFormat == FileNameFormat.PublishedURLName)
            {
                string url = asset.FileProperties.PublishedURL;
                Uri uri = new Uri(url);
                string fileName = Path.GetFileName(uri.LocalPath);
                return fileName;
            }
            return asset.FileProperties.FileName;
        }

        private void AskFileNameFormat()
        {
            if (_fileNameFormat == null)
            {
                Console.WriteLine($"QUESTION! What file name format do you want to use?");
                Console.WriteLine($"1. Original File Name (Default)");
                Console.WriteLine($"2. Published URL Name");

                Console.WriteLine($"Please enter 1 or 2");

                string? input = Console.ReadLine();
                if (input == "1")
                {
                    _fileNameFormat = FileNameFormat.OriginalFileName;
                }
                else if (input == "2")
                {
                    _fileNameFormat = FileNameFormat.PublishedURLName;
                }
                else
                {
                    Console.WriteLine($"Invalid input. Defaulting to Original File Name.");
                    _fileNameFormat = FileNameFormat.OriginalFileName;
                }
                _fileNameFormat = FileNameFormat.PublishedURLName;
            }
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

        
        
        private async Task<List<AssetPoco>> GetAssetsByFolderAsync(int folderId)
        {
            int page = 1;
            int currentPageSize = 0;
            var retval = new List<AssetPoco>();
            do
            {
                try
                {
                    var results = await LoadPageOfAssetsAsync
                    (
                        folderId,
                        page
                    );
                    if (results != null)
                    {
                        currentPageSize = ProcessSfmcAssets(folderId, retval, results!);
                    }
                }
                catch (System.Exception ex)
                {
                    _logger.LogError(ex, "Failed loading assets for image download. FolderId={FolderId}", folderId);
                    break;
                }
                page++;
            }
            while (AppConfiguration.Instance.PageSize == currentPageSize);

            if (!retval.Any())
            {
                throw new Exception("Error Loading Folders");
            }

            return retval;
        }

        private int ProcessSfmcAssets(int folderId, List<AssetPoco> retval, RestResults<SfmcRestWrapper<SfmcAsset>, string> results)
        {
            int currentPageSize = results!.Results.items.Count();
            if (currentPageSize > 0)
            {
                var sfmcAssets = results!.Results.items;
                _logger.LogInformation("Loaded {AssetCount} assets for image folder {FolderId}.", sfmcAssets.Count(), folderId);
                foreach (SfmcAsset sfmcAsset in sfmcAssets)
                {
                    var asset = sfmcAsset.ToPoco();
                    retval.Add(asset);
                    //asset.FullPath = folderId.FullPath;
                    //folderId.Assets.Add(asset);
                }
            }

            _logger.LogDebug("Image asset page processed for folder {FolderId}. PageCount={PageCount}, AggregateCount={AggregateCount}", folderId, currentPageSize, retval.Count);

            if (AppConfiguration.Instance.PageSize == currentPageSize)
            {
                _logger.LogTrace("More image asset pages remain for folder {FolderId}.", folderId);
            }

            return currentPageSize;
        }

        private async Task<RestResults<SfmcRestWrapper<SfmcAsset>, string>?> LoadPageOfAssetsAsync(int folderId, int page)
        {
            var token = await _authRepository.GetTokenAsync();
            _logger.LogDebug("Loading image asset page {PageNumber} for folder {FolderId}.", page, folderId);
            string uri = $"{bleak.Martech.SalesforceMarketingCloud.Configuration.SfmcEndpointUrls.GetRestEndpoint(AppConfiguration.Instance.Subdomain, "/asset/v1/content/assets", AppConfiguration.Instance.RestBaseUrl)}?$page={page}&$pagesize={AppConfiguration.Instance.PageSize}&$orderBy=name&$filter=category.id eq {folderId}";

            _logger.LogTrace("Requesting image assets. FolderId={FolderId}, Uri={Uri}", folderId, uri);

            var results = await _restClient.ExecuteRestMethodAsync<SfmcRestWrapper<SfmcAsset>, string>(
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
                _logger.LogWarning("Image asset request returned unauthenticated response. FolderId={FolderId}, Error={Error}", folderId, results.UnhandledError);
                token = await _authRepository.GetTokenAsync();

                results = await _restClient.ExecuteRestMethodAsync<SfmcRestWrapper<SfmcAsset>, string>(
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

            _logger.LogTrace("Image asset request completed. FolderId={FolderId}, HasResults={HasResults}", folderId, results?.Results != null);
            if (results?.Error != null) _logger.LogWarning("Image asset request returned an error. FolderId={FolderId}, Error={Error}", folderId, results.Error);
            return results;
        }
    }
}
