using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Configuration;
using bleak.Martech.SalesforceMarketingCloud.Wsdl;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Authentication.SfmcPocos;
using bleak.Martech.SalesforceMarketingCloud.ContentBuilder;
using bleak.Martech.SalesforceMarketingCloud.ContentBuilder.SfmcPocos;
using System.Collections.Generic;
using System;
using System.Diagnostics;

namespace bleak.Martech.SalesforceMarketingCloud.ConsoleApp
{
    public static class Program
    {
        static JsonSerializer serializer = new JsonSerializer();
        static RestManager rm = new RestManager(serializer, serializer);
        static SfmcAuthToken token = new();
        private static int assetCounter = 0;
        private static int folderCounter = 0;
        private static HashSet<string> assetTypes = new HashSet<string>();

        private static void Main(string[] args)
        {
            // Create a Stopwatch instance
            Stopwatch stopwatch = new Stopwatch();

            // Start measuring time
            stopwatch.Start();
            // See https://aka.ms/new-console-template for more information
            Console.WriteLine($"Getting Auth Token");
            ResolveAuthentication();

            if (AppConfiguration.Instance.Debug) Console.WriteLine($"Gotten Auth Token {token.access_token}");

            Console.WriteLine($"Getting Folder Tree");
            Console.WriteLine("---------------------");
            var folderTree = GetFolderTree();
            Console.WriteLine($"Completed Building Folder Tree");
            Console.WriteLine("---------------------");

            // PrintChildren(folderTree);
            // PrintFolders(folderTree);
            DownloadAllAssets(folderTree);

            // Stop the Stopwatch
            stopwatch.Stop();

            // Display the elapsed time
            Console.WriteLine($"Execution Time: {stopwatch.ElapsedMilliseconds} ms");
        
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
                Directory.CreateDirectory(Path.GetDirectoryName(metaDataCustomerKey));
                if (AppConfiguration.Instance.Debug) { Console.WriteLine($"Writing Metadata file to to save {metaDataCustomerKey}"); }
                File.WriteAllText(metaDataCustomerKey, serializer.Serialize(asset));

                // Write Metadata to Named as Id
                var metaDataId = AppConfiguration.Instance.OutputFolder + "/" + asset.FullPath + "/id-" + asset.Id + ".metadata";
                Directory.CreateDirectory(Path.GetDirectoryName(metaDataId));
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

        private static void ResolveAuthentication()
        {
            string authFile = Path.Combine(AppContext.BaseDirectory, "authentication.json");
            double threshold = 600.07;
            if (File.Exists(authFile))
            {
                // Get the last write time of the file
                DateTime lastWriteTime = File.GetLastWriteTime(authFile);

                // Calculate the time difference
                TimeSpan timeDifference = DateTime.Now - lastWriteTime;

                // Check if the file is older than 600 seconds
                if (timeDifference.TotalSeconds > threshold)
                {
                    Console.WriteLine("The file is older than 600 seconds.");
                    Console.WriteLine($"Deleting file {authFile}");
                    File.Delete(authFile);
                    Console.WriteLine($"Authenticating");
                    token = Authenticate();
                    if (AppConfiguration.Instance.Debug) Console.WriteLine($"Authenticated: {token.access_token}");
                    string json = serializer.Serialize(token);
                    if (AppConfiguration.Instance.Debug) Console.WriteLine($"Writing file {authFile}");
                    File.WriteAllText(authFile, json);
                    Thread.Sleep(1000);
                }
                else
                {
                    Console.WriteLine($"The file is not older than 600 seconds. timeDifference.TotalSeconds: {timeDifference.TotalSeconds}; threshold: {threshold}");
                    token = serializer.Deserialize<SfmcAuthToken>(File.ReadAllText(authFile));
                }
            }
            else
            {
                Console.WriteLine("No file exists");
                Console.WriteLine($"Authenticating");
                token = Authenticate();
                if (AppConfiguration.Instance.Debug) Console.WriteLine($"Authenticated: {token.access_token}");
                string json = serializer.Serialize(token);
                Console.WriteLine($"Writing file {authFile}");
                File.WriteAllText(authFile, json);
                Thread.Sleep(1000);
            }
        }

        private static SfmcAuthToken Authenticate()
        {
            Console.WriteLine($"Authenticating...........");
            RestResults<SfmcAuthToken, string> authResults;

            string tokenUri = "https://" + AppConfiguration.Instance.Subdomain + ".auth.marketingcloudapis.com/v2/token";
            Console.WriteLine($"Trying to authenticate to {tokenUri}");

            authResults = rm.ExecuteRestMethod<SfmcAuthToken, string>(
                uri: new Uri(tokenUri),
                verb: HttpVerbs.POST,
                payload: new
                {
                    grant_type = "client_credentials",
                    client_id = AppConfiguration.Instance.ClientId,
                    client_secret = AppConfiguration.Instance.ClientSecret,
                    account_id = AppConfiguration.Instance.MemberId
                },
                headers: new List<Header>() { new Header() { Name = "Content-Type", Value = "application/json" } }
                );

            //File.WriteAllBytes("temp.token", authResults.Results.)
            if (AppConfiguration.Instance.Debug) Console.WriteLine($"authResults.Value = {authResults.Results}");
            if (authResults.Error != null) Console.WriteLine($"authResults.Error = {authResults.Error}");

            return authResults.Results;
        }

        private static List<FolderObject> GetFolderTree()
        {
            int page = 1;
            int currentPageSize = 0;
            
            var sfmcFolders = new List<SfmcFolder>();
            do
            {
                currentPageSize = LoadFolder(page, sfmcFolders);
                page++;
            }
            while (AppConfiguration.Instance.PageSize == currentPageSize);

            if (sfmcFolders.Any())
            {
                return BuildFolderTree(sfmcFolders);
            }

            throw new Exception("Error Loading Folders");
        }

        private static int LoadFolder(int page, List<SfmcFolder> sfmcFolders)
        {
            int currentPageSize;
            try
            {
                if (AppConfiguration.Instance.Debug) { Console.WriteLine($"Loading Folder Page #{page}"); }
                // https://{{et_subdomain}}.rest.marketingcloudapis.com/asset/v1/content/categories?$pagesize=107&$page=4
                string uri = $"https://{AppConfiguration.Instance.Subdomain}.rest.marketingcloudapis.com/asset/v1/content/categories?$page={page}&$pagesize={AppConfiguration.Instance.PageSize}";
                //string uri = $"https://{AppConfiguration.Instance.Subdomain}.rest.marketingcloudapis.com/asset/v1/content/categories?page={page}&pagesize={pageSize}$orderBy=name&$filter=parentId eq {parent_id}";
                if (AppConfiguration.Instance.Debug) { Console.WriteLine($"Trying to download to {uri} with {token.access_token}"); }

                var results = rm.ExecuteRestMethod<SfmcRestWrapper<SfmcFolder>, string>(
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
                    ResolveAuthentication();

                    results = rm.ExecuteRestMethod<SfmcRestWrapper<SfmcFolder>, string>(
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

                currentPageSize = results.Results.items.Count();
                sfmcFolders.AddRange(results.Results.items);
                if (AppConfiguration.Instance.Debug) Console.WriteLine($"Current Page had {currentPageSize} records. There are now {sfmcFolders.Count()} Total Folders Identified.");

                if (AppConfiguration.Instance.PageSize == currentPageSize)
                {
                    if (AppConfiguration.Instance.Debug) Console.WriteLine($"Running Loop Again");
                }

            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                throw;
            }

            return currentPageSize;
        }

        private static void GetAssetsByFolder(FolderObject folderObject)
        {
            int page = 1;
            int currentPageSize = 0;
            
            var sfmcAssets = new List<SfmcAsset>();
            do
            {
                try
                {
                    if (AppConfiguration.Instance.Debug) Console.WriteLine($"Loading Assets Page #{page}");
                    //string url = $"https://{AppConfiguration.Instance.Subdomain}.rest.marketingcloudapis.com/asset/v1/content/assets?$page=1&$pagesize=100&$orderBy=name asc&$filter=category.id=5843
                    string uri = $"https://{AppConfiguration.Instance.Subdomain}.rest.marketingcloudapis.com/asset/v1/content/assets?$page={page}&$pagesize={AppConfiguration.Instance.PageSize}&$orderBy=name&$filter=category.id eq {folderObject.Id}";

                    if (AppConfiguration.Instance.Debug) Console.WriteLine($"Trying to download to {uri} with {token.access_token}");

                    var results = rm.ExecuteRestMethod<SfmcRestWrapper<SfmcAsset>, string>(
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
                        ResolveAuthentication();

                        results = rm.ExecuteRestMethod<SfmcRestWrapper<SfmcAsset>, string>(
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
                    currentPageSize = results.Results.items.Count();
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
                    var asset = sfmcAsset.ToAssetObject();
                    asset.FullPath = folderObject.FullPath;
                    folderObject.Assets.Add(asset);
                }
            }

//            throw new Exception("Error Loading Folders");

        }

        public static List<FolderObject> BuildFolderTree(List<SfmcFolder> sfmcFolders)
        {
            const int root_folder = 0;

            // Find root folders
            var sfmcRoots = sfmcFolders.Where(f => f.parentId == root_folder).ToList();
            var retval = new List<FolderObject>();
            foreach (var sfmcRoot in sfmcRoots)
            {
                var folderObject = sfmcRoot.ToFolderObject();
                folderObject.FullPath = "/";
                GetAssetsByFolder(folderObject);
                AddChildren(folderObject, sfmcFolders);
                retval.Add(folderObject);
            }
            return retval;
        }


        private static void AddChildren(FolderObject parentFolder, List<SfmcFolder> sfmcFolders)
        {
            var sfmcFolders_w_MatchingParentId = sfmcFolders.Where(x => x.parentId == parentFolder.Id).ToList();
            if (sfmcFolders_w_MatchingParentId.Any())
            {
                parentFolder.SubFolders = new List<FolderObject>();
                foreach (var sfmcFolder in sfmcFolders_w_MatchingParentId)
                {
                    var subfolder = sfmcFolder.ToFolderObject();
                    subfolder.FullPath = parentFolder.FullPath + subfolder.Name + "/";
                    GetAssetsByFolder(subfolder);
                    AddChildren(subfolder, sfmcFolders);
                    parentFolder.SubFolders.Add(subfolder);
                }
            }
        }

    }
}