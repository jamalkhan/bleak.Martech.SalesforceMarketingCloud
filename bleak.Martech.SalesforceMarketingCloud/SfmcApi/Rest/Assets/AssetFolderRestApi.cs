using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.Authentication;
using bleak.Martech.SalesforceMarketingCloud.Models.SfmcDtos;
using bleak.Martech.SalesforceMarketingCloud.Models;
using bleak.Martech.SalesforceMarketingCloud.Configuration;
using bleak.Martech.SalesforceMarketingCloud.Rest;
using Microsoft.Extensions.Logging;

namespace bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Sfmc.Rest.Assets
{
    public class AssetFolderRestApi
    : BaseRestApi
    , IAssetFolderRestApi
    {
        private readonly ILogger<AssetFolderRestApi> _logger;

        private HttpVerbs verb = HttpVerbs.GET;

        public AssetFolderRestApi(
            IAuthRepository authRepository,
            SfmcConnectionConfiguration config,
            ILogger<AssetFolderRestApi> logger
            )
            : base(authRepository: authRepository, config: config)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            if (config == null) config = new SfmcConnectionConfiguration();
            if (config.PageSize > 500)
            {
                _logger.LogWarning($"PageSize is set to {config.PageSize}, which exceeds the maximum allowed value of 500. Setting PageSize to 500.");
                base._sfmcConnectionConfiguration.PageSize = 500; // Set a reasonable default max page size
            }
        }


        public async Task<List<FolderObject>> GetFolderTreeAsync()
        {
            _logger.LogInformation("GetFolderTreeAsync called");
            return await Task.Run(() => GetFolderTree());
        }

        public List<FolderObject> GetFolderTree()
        {
            _logger.LogInformation("GetFolderTree() invoked");
            int page = 1;
            int currentPageSize = 0;

            var sfmcFolders = new List<SfmcFolder>();
            do
            {
                _logger.LogTrace($"Executing GetFolderTree() page: {page}");
                currentPageSize = LoadFolder(page, sfmcFolders);
                page++;
                _logger.LogInformation($"LoadFolder() returned currentPageSize: {currentPageSize} and moving onto page {page}.");
            }
            while (_sfmcConnectionConfiguration.PageSize == currentPageSize);

            if (sfmcFolders.Any())
            {
                return BuildFolderTree(sfmcFolders);
            }
            _logger.LogError("Error Loading Folders");
            throw new Exception("Error Loading Folders");
        }

        List<FolderObject> BuildFolderTree(List<SfmcFolder> sfmcFolders)
        {
            const int root_folder = 0;

            // Find root folders
            var sfmcRoots = sfmcFolders.Where(f => f.parentId == root_folder).ToList();
            var retval = new List<FolderObject>();
            foreach (var sfmcRoot in sfmcRoots)
            {
                var folderObject = sfmcRoot.ToFolderObject();
                folderObject.FullPath = "/";
                AddChildren(folderObject, folderObject.FullPath, sfmcFolders);
                retval.Add(folderObject);
                _logger.LogInformation($"Added root folder: {folderObject.Name} (ID: {folderObject.Id}) with FullPath: {folderObject.FullPath} with {folderObject.SubFolders?.Count ?? 0} subfolders.");
            }
            return retval;
        }

        private void AddChildren(FolderObject parentFolder, string path, List<SfmcFolder> sfmcFolders)
        {
            var childFolders = sfmcFolders
                .Where(f => f.parentId == parentFolder.Id)
                .OrderBy(f => f.name);
            if (!childFolders.Any())
            {
                _logger.LogTrace($"No child folders found for parent folder: {parentFolder.Name} (ID: {parentFolder.Id})");
                return;
            }
            

            _logger.LogTrace($"Found {childFolders.Count()} child folders for parent folder: {parentFolder.Name} (ID: {parentFolder.Id})");
            parentFolder.SubFolders = new List<FolderObject>();

            foreach (var childFolder in childFolders)
            {
                var childFolderObject = childFolder.ToFolderObject();
                childFolderObject.FullPath =  path + childFolderObject.Name + "/";
                _logger.LogTrace($"Adding child folder: {childFolderObject.Name} (ID: {childFolderObject.Id}) to parent folder: {parentFolder.Name} (ID: {parentFolder.Id})");
                parentFolder.SubFolders.Add(childFolderObject);

                // Recursively add children to the child folder
                AddChildren(
                    parentFolder: childFolderObject,
                    path: childFolderObject.FullPath,
                    sfmcFolders: sfmcFolders);
            }
        }

        private int LoadFolder(int page, List<SfmcFolder> sfmcFolders)
        {
            int currentPageSize;
            try
            {
                RestResults<SfmcRestWrapper<SfmcFolder>, string> results;
                //asset/v1/content/categories
                string url = $"https://{_authRepository.Subdomain}.rest.marketingcloudapis.com/asset/v1/content/categories/?$page={page}&$pagesize={_sfmcConnectionConfiguration.PageSize}";
                _logger.LogInformation($"Loading Folder Page #{page} with URL: {url}");
                results = ExecuteRestMethodWithRetry(
                    loadFolderApiCall: LoadFolderApiCall,
                    url: url,
                    authenticationError: "401",
                    resolveAuthentication: _authRepository.ResolveAuthentication
                );

                _logger.LogTrace($"results.Value = {results?.Results}");
                _logger.LogError($"results.Error = {results?.Error}");

                currentPageSize = results!.Results.items.Count();
                sfmcFolders.AddRange(results.Results.items);
                _logger.LogInformation($"Current Page had {currentPageSize} records. There are now {sfmcFolders.Count()} Total Folders Identified.");

                if (_sfmcConnectionConfiguration.PageSize == currentPageSize)
                {
                    _logger.LogInformation($"Running Loop Again");
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"{ex.Message}");
                throw;
            }

            return currentPageSize;
        }

        private RestResults<SfmcRestWrapper<SfmcFolder>, string> LoadFolderApiCall(
            string url
        )
        {
            if (_sfmcConnectionConfiguration.Debug) { Console.WriteLine($"Attempting to {verb} to {url} with accessToken: {_authRepository.Token.access_token}"); }

            SetAuthHeader();
            var results = _restManager.ExecuteRestMethod<SfmcRestWrapper<SfmcFolder>, string>(
                uri: new Uri(url),
                verb: verb,
                headers: _headers
                );

            return results!;
        }

        private RestResults<SfmcRestWrapper<SfmcFolder>, string> ExecuteRestMethodWithRetry(
            Func<string, RestResults<SfmcRestWrapper<SfmcFolder>, string>> loadFolderApiCall,
            string url,
            string authenticationError,
            Action resolveAuthentication
            )
        {
            var results = loadFolderApiCall(url);

            // Check if an error occurred and it matches the specified errorText
            if (results != null && results.UnhandledError != null && results.UnhandledError.Contains(authenticationError))
            {
                Console.WriteLine($"Unauthenticated: {results.UnhandledError}");

                // Resolve authentication
                resolveAuthentication();
                Console.WriteLine("Authentication Header has been reset");

                // Retry the REST method
                results = loadFolderApiCall(url);

                Console.WriteLine("Press Enter to Continue");
                Console.ReadLine();
            }

            return results!;
        }
    }
}