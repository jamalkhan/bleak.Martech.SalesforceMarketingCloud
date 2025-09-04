using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.Authentication;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Configuration;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Sfmc.Soap;
using bleak.Martech.SalesforceMarketingCloud.Models;
using bleak.Martech.SalesforceMarketingCloud.Models.Helpers;
using bleak.Martech.SalesforceMarketingCloud.Models.SfmcDtos;

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
        public DownloadContentApp(IRestClientAsync restClientAsync, IAuthRepository authRepository)
        {
            _restClientAsync = restClientAsync;
            _authRepository = authRepository;
        }

        public async Task Execute()
        {
            Console.WriteLine($"Getting Folder Tree");
            Console.WriteLine("---------------------");
            LoadFolders loadFolders = new LoadFolders(restClientAsync: _restClientAsync, authRepository: _authRepository);
            var folderTree = await loadFolders.GetFolderTreeAsync();
            Console.WriteLine($"Completed Building Folder Tree");
            Console.WriteLine("---------------------");
            // PrintChildren(folderTree);
            // PrintFolders(folderTree);
            DownloadAllAssets(folderTree);
        }

        private static void DownloadAllAssets(List<FolderObject> folderTree)
        {
            Console.WriteLine("Download All Assets");
            Console.WriteLine("---------------------");

            var path = AppConfiguration.Instance.OutputFolder;
            if (AppConfiguration.Instance.Debug) Console.WriteLine($"Creating Directory {path}");
            Directory.CreateDirectory(path);
            if (AppConfiguration.Instance.Debug) Console.WriteLine($"Directory Created {path}");

            foreach (var folder in folderTree)
            {
                assetCounter = DownloadAllAssetsForAFolder(folder);
                folderCounter++;
                Console.WriteLine($"Processed {folderCounter} folders...");
                
            }
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

        private static int DownloadAllAssetsForAFolder(FolderObject folder)
        {
            foreach (var asset in folder.Assets)
            {
                assetTypes.Add(asset.AssetType.Name);

                // Write Metadata to Named as Customer Key
                var metaDataCustomerKey = AppConfiguration.Instance.OutputFolder + "/" + asset.FullPath + "/customerkey-" + asset.CustomerKey + ".metadata";
                Directory.CreateDirectory(Path.GetDirectoryName(metaDataCustomerKey)!);
                if (AppConfiguration.Instance.Debug) { Console.WriteLine($"Writing Metadata file to to save {metaDataCustomerKey}"); }
                File.WriteAllText(metaDataCustomerKey, serializer.Serialize(asset));

                // Write Metadata to Named as Id
                var metaDataId = AppConfiguration.Instance.OutputFolder + "/" + asset.FullPath + "/id-" + asset.Id + ".metadata";
                Directory.CreateDirectory(Path.GetDirectoryName(metaDataId)!);
                if (AppConfiguration.Instance.Debug) { Console.WriteLine($"Writing Metadata file to to save {metaDataId}"); }
                File.WriteAllText(metaDataId, serializer.Serialize(asset));

                string outputFileName = string.Empty;
                switch (asset.AssetType.Name)
                {
                    case "codesnippetblock":
                        outputFileName = AppConfiguration.Instance.OutputFolder + "/" + asset.FullPath + "/customerkey-" + asset.CustomerKey + ".ampscript.html";
                        if (AppConfiguration.Instance.Debug) { Console.WriteLine($"Trying to save {outputFileName}"); }
                        File.WriteAllText(outputFileName, asset.Content);

                        outputFileName = AppConfiguration.Instance.OutputFolder + "/" + asset.FullPath + "/id-" + asset.Id + ".ampscript.html";
                        if (AppConfiguration.Instance.Debug) { Console.WriteLine($"Trying to save {outputFileName}"); }
                        File.WriteAllText(outputFileName, asset.Content);
                        break;
                    case "webpage":
                    case "htmlemail":
                    case "htmlblock":
                    case "templatebasedemail":
                        outputFileName = AppConfiguration.Instance.OutputFolder + "/" + asset.FullPath + "/customerkey-" + asset.CustomerKey + ".ampscript.html";
                        if (AppConfiguration.Instance.Debug) { Console.WriteLine($"Trying to save {outputFileName}"); }
                        File.WriteAllText(outputFileName, asset.Views.Html.Content);

                        outputFileName = AppConfiguration.Instance.OutputFolder + "/" + asset.FullPath + "/id-" + asset.Id + ".ampscript.html";
                        if (AppConfiguration.Instance.Debug) { Console.WriteLine($"Trying to save {outputFileName}"); }
                        File.WriteAllText(outputFileName, asset.Views.Html.Content);
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
                    if (AppConfiguration.Instance.Debug) Console.WriteLine($"Loading Assets Page #{page}");
                    //string url = $"https://{AppConfiguration.Instance.Subdomain}.rest.marketingcloudapis.com/asset/v1/content/assets?$page=1&$pagesize=100&$orderBy=name asc&$filter=category.id=5843
                    string uri = $"https://{AppConfiguration.Instance.Subdomain}.rest.marketingcloudapis.com/asset/v1/content/assets?$page={page}&$pagesize={AppConfiguration.Instance.PageSize}&$orderBy=name&$filter=category.id eq {folderObject.Id}";

                    if (AppConfiguration.Instance.Debug) Console.WriteLine($"Trying to download to {uri} with {token.access_token}");

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
                        Console.WriteLine($"Unauthenticated: {results.UnhandledError}");
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

                    if (AppConfiguration.Instance.Debug) Console.WriteLine($"results.Value = {results?.Results}");
                    if (results?.Error != null) Console.WriteLine($"results.Error = {results.Error}");
                    
                    currentPageSize = results!.Results.items.Count();
                    sfmcAssets.AddRange(results.Results.items);
                    if (AppConfiguration.Instance.Debug) Console.WriteLine($"Current Page had {currentPageSize} records. There are now {sfmcAssets.Count()} Total Assets Identified in {folderObject.FullPath}.");

                    if (AppConfiguration.Instance.PageSize == currentPageSize)
                    {
                        if (AppConfiguration.Instance.Debug) Console.WriteLine($"Running Loop Again");
                    }

                }
                catch (System.Exception ex)
                {
                    Console.WriteLine($"{ex.Message}");

                    break;
                }
                page++;
            }
            while (AppConfiguration.Instance.PageSize == currentPageSize);


            if (sfmcAssets.Any())
            {
                Console.WriteLine($"There are {sfmcAssets.Count()} assets in {folderObject.FullPath}");
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