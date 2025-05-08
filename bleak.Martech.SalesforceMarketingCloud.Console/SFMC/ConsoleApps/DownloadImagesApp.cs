using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Configuration;
using bleak.Martech.SalesforceMarketingCloud.Authentication;
using bleak.Martech.SalesforceMarketingCloud.Models;
using bleak.Martech.SalesforceMarketingCloud.Models.SfmcDtos;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Sfmc.Soap;
using System.Diagnostics;
using System;
using System.IO;
using bleak.Martech.SalesforceMarketingCloud.Wsdl;

namespace bleak.Martech.SalesforceMarketingCloud.ConsoleApp.ConsoleApps
{

    public class DownloadImagesApp : IConsoleApp
    {
        static JsonSerializer serializer = new JsonSerializer();
        private static int assetCounter = 0;
        private static HashSet<string> assetTypes = new HashSet<string>();
        RestManager _restManager;
        IAuthRepository _authRepository;
        string fullPath = string.Empty;
        public DownloadImagesApp(RestManager restManager, IAuthRepository authRepository)
        {
            _restManager = restManager;
            _authRepository = authRepository;
        }

        public void Execute()
        {
            Console.WriteLine("Give me the folder ID");
            var folder = Console.ReadLine();
            
            int.TryParse(folder, out int folderId);
            DownloadAllAssets(folderId);
        }



        private void CreateFolder()
        {
            // Get the path to the user's desktop
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            Console.WriteLine($"desktopPath = {desktopPath}");

            // Define the name of the new folder
            string newFolderName = "MyNewFolder";

            // Combine to get the full path
            fullPath = Path.Combine(desktopPath, newFolderName);
            Console.WriteLine($"fullPath = {fullPath}");

            if (AppConfiguration.Instance.Debug) Console.WriteLine($"Creating Directory {fullPath}");
            Directory.CreateDirectory(fullPath);
            if (AppConfiguration.Instance.Debug) Console.WriteLine($"Directory Created {fullPath}");
        }


        private void DownloadAllAssets(int folderId)
        {
            CreateFolder();

            Console.WriteLine("Download All Assets");
            Console.WriteLine("---------------------");

            assetCounter = DownloadAllAssetsForAFolder(folderId);

            
            Console.WriteLine("Assets have been downloaded");
            Console.WriteLine("---------------------");

            Console.WriteLine("---------------------");
            Console.WriteLine("The following assettypes were found:");
            foreach (var assetType in assetTypes)
            {
                Console.WriteLine(assetType);
            }
            Console.WriteLine("---------------------");
            
        }

        private int DownloadAllAssetsForAFolder(int folderId)
        {
            var assets = GetAssetsByFolder(folderId);

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
                        if (AppConfiguration.Instance.Debug) { Console.WriteLine($"Trying to save {metadataFile}"); }
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
                                if (AppConfiguration.Instance.Debug) { Console.WriteLine($"Image saved to {imageFilePath}"); }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Failed to download image from {imageUrl}: {ex.Message}");
                            }
                        }



                        break;
                    default:
                        break;
                }

                assetCounter++;
                if (assetCounter % AppConfiguration.Instance.PageSize == 0)
                {
                    Console.WriteLine($"Wrote {assetCounter} assets to the filesystem...");
                }
            }

            return assetCounter;
        }


        enum FileNameFormat{
            OriginalFileName,
            PublishedURLName,
        }
        private FileNameFormat? _fileNameFormat = null; 
        private string GetFileName(AssetObject asset)
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

        
        
        private List<AssetObject> GetAssetsByFolder(int folderId)
        {
            int page = 1;
            int currentPageSize = 0;
            var retval = new List<AssetObject>();
            do
            {
                try
                {
                    RestResults<SfmcRestWrapper<SfmcAsset>, string>? results = LoadPageOfAssets(folderId, page);
                    currentPageSize = ProcessSfmcAssets(folderId, retval, results);
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine($"{ex.Message}");
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

        private static int ProcessSfmcAssets(int folderId, List<AssetObject> retval, RestResults<SfmcRestWrapper<SfmcAsset>, string> results)
        {
            int currentPageSize = results!.Results.items.Count();
            if (currentPageSize > 0)
            {
                var sfmcAssets = results!.Results.items;
                Console.WriteLine($"There are {sfmcAssets.Count()} assets in folderId {folderId}");
                foreach (SfmcAsset sfmcAsset in sfmcAssets)
                {
                    var asset = sfmcAsset.ToAssetObject();
                    retval.Add(asset);
                    //asset.FullPath = folderId.FullPath;
                    //folderId.Assets.Add(asset);
                }
            }

            if (AppConfiguration.Instance.Debug) Console.WriteLine($"Current Page had {currentPageSize} records. There are now {retval.Count()} Total Assets Identified in folderId={folderId}.");

            if (AppConfiguration.Instance.PageSize == currentPageSize)
            {
                if (AppConfiguration.Instance.Debug) Console.WriteLine($"Running Loop Again");
            }

            return currentPageSize;
        }

        private RestResults<SfmcRestWrapper<SfmcAsset>, string>? LoadPageOfAssets(int folderId, int page)
        {
            if (AppConfiguration.Instance.Debug) Console.WriteLine($"Loading Assets Page #{page}");
            string uri = $"https://{AppConfiguration.Instance.Subdomain}.rest.marketingcloudapis.com/asset/v1/content/assets?$page={page}&$pagesize={AppConfiguration.Instance.PageSize}&$orderBy=name&$filter=category.id eq {folderId}";

            if (AppConfiguration.Instance.Debug) Console.WriteLine($"Trying to download to {uri} with {_authRepository.Token.access_token}");

            var results = _restManager.ExecuteRestMethod<SfmcRestWrapper<SfmcAsset>, string>(
                uri: new Uri(uri),
                verb: HttpVerbs.GET,
                headers:
                    new List<Header>()
                    {
                        new Header() { Name = "Content-Type", Value = "application/json" } ,
                        new Header() { Name = "Authorization", Value = $"Bearer {_authRepository.Token.access_token}" }
                    }
                );

            if (results != null && results.UnhandledError != null && results.UnhandledError.Contains("401"))
            {
                Console.WriteLine($"Unauthenticated: {results.UnhandledError}");
                _authRepository.ResolveAuthentication();

                results = _restManager.ExecuteRestMethod<SfmcRestWrapper<SfmcAsset>, string>(
                    uri: new Uri(uri),
                    verb: HttpVerbs.GET,
                    headers:
                        new List<Header>()
                        {
                            new Header() { Name = "Content-Type", Value = "application/json" } ,
                            new Header() { Name = "Authorization", Value = $"Bearer {_authRepository.Token.access_token}" }
                        }
                    );
            }

            if (AppConfiguration.Instance.Debug) Console.WriteLine($"results.Value = {results?.Results}");
            if (results?.Error != null) Console.WriteLine($"results.Error = {results.Error}");
            return results;
        }
    }
}