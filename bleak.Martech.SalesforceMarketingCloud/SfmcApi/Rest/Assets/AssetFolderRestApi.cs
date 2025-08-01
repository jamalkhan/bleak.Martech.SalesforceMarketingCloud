using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.Authentication;
using bleak.Martech.SalesforceMarketingCloud.Models.SfmcDtos;
using bleak.Martech.SalesforceMarketingCloud.Models;
using bleak.Martech.SalesforceMarketingCloud.Configuration;
using bleak.Martech.SalesforceMarketingCloud.Rest;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace bleak.Martech.SalesforceMarketingCloud.Sfmc.Rest.Assets
{
    public class AssetFolderRestApi
    : BaseRestApi
    , IAssetFolderRestApi
    {
        private readonly ILogger<AssetFolderRestApi> _logger;

        private HttpVerbs verb = HttpVerbs.GET;

        public AssetFolderRestApi(
            IAuthRepository authRepository,
            SfmcConnectionConfiguration sfmcConnectionConfiguration,
            ILogger<AssetFolderRestApi> logger
            )
            : base(
                restManager: new RestManager(new JsonSerializer(), new JsonSerializer()),
                restManagerAsync: new RestManager(new JsonSerializer(), new JsonSerializer()),
                authRepository: authRepository,
                sfmcConnectionConfiguration: sfmcConnectionConfiguration
            )
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            if (sfmcConnectionConfiguration == null) sfmcConnectionConfiguration = new SfmcConnectionConfiguration();
            if (sfmcConnectionConfiguration.PageSize > 500)
            {
                _logger.LogWarning($"PageSize is set to {sfmcConnectionConfiguration.PageSize}, which exceeds the maximum allowed value of 500. Setting PageSize to 500.");
                base._sfmcConnectionConfiguration.PageSize = 500; // Set a reasonable default max page size
            }
        }


        public async Task<List<FolderObject>> GetFolderTreeAsync()
        {
            _logger.LogTrace("GetFolderTreeAsync called");

            int page = 1;
            int currentPageSize = 0;

            var sfmcFolders = new List<SfmcFolder>();
            do
            {
                _logger.LogTrace($"Executing GetFolderTreeAsync page: {page}");
                currentPageSize = await LoadFolderAsync(page, sfmcFolders);
                page++;
                _logger.LogTrace($"LoadFolderAsync returned currentPageSize: {currentPageSize} and moving onto page {page}.");
            }
            while (_sfmcConnectionConfiguration.PageSize == currentPageSize);

            _logger.LogInformation($"LoadFolderAsync loaded {sfmcFolders.Count} Folders");
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
                _logger.LogTrace($"Added root folder: {folderObject.Name} (ID: {folderObject.Id}) with FullPath: {folderObject.FullPath} with {folderObject.SubFolders?.Count ?? 0} subfolders.");
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

        private async Task<int> LoadFolderAsync(int page, List<SfmcFolder> sfmcFolders)
        {
            int currentPageSize;
            try
            {
                RestResults<SfmcRestWrapper<SfmcFolder>, string> results;
                //asset/v1/content/categories
                string url = $"https://{_authRepository.Subdomain}.rest.marketingcloudapis.com/asset/v1/content/categories/?$page={page}&$pagesize={_sfmcConnectionConfiguration.PageSize}";
                _logger.LogTrace($"Loading Folder Page #{page} with URL: {url}");
                results = await ExecuteRestMethodWithRetryAsync(
                    loadFolderApiCallAsync: LoadFolderApiCallAsync,
                    url: url,
                    authenticationError: "401",
                    resolveAuthenticationAsync: _authRepository.ResolveAuthenticationAsync
                );

                _logger.LogTrace($"results.Value = {results?.Results}");
                if (results?.Error != null) _logger.LogError($"results.Error = {results?.Error}");

                currentPageSize = results!.Results.items.Count();
                sfmcFolders.AddRange(results.Results.items);
                _logger.LogTrace($"Current Page had {currentPageSize} records. There are now {sfmcFolders.Count()} Total Folders Identified.");

                if (_sfmcConnectionConfiguration.PageSize == currentPageSize)
                {
                    _logger.LogTrace($"Running Loop Again");
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"{ex.Message}");
                throw;
            }

            return currentPageSize;
        }

        private async Task<RestResults<SfmcRestWrapper<SfmcFolder>, string>> LoadFolderApiCallAsync(string url)
        {
            _logger.LogTrace($"Attempting to {verb} to {url} with accessToken: {_authRepository.Token.access_token}");

            SetAuthHeader();
            var results = await _restManagerAsync.ExecuteRestMethodAsync<SfmcRestWrapper<SfmcFolder>, string>(
                uri: new Uri(url),
                verb: verb,
                headers: _headers
            );

            return results!;
        }

        private async Task<RestResults<SfmcRestWrapper<SfmcFolder>, string>> ExecuteRestMethodWithRetryAsync(
            Func<string, Task<RestResults<SfmcRestWrapper<SfmcFolder>, string>>> loadFolderApiCallAsync,
            string url,
            string authenticationError,
            Func<Task> resolveAuthenticationAsync
            )
        {
            var results = await loadFolderApiCallAsync(url);

            // Check if an error occurred and it matches the specified errorText
            if (results != null && results.UnhandledError != null && results.UnhandledError.Contains(authenticationError))
            {
                _logger.LogTrace($"Unauthenticated: {results.UnhandledError}");

                // Resolve authentication
                await resolveAuthenticationAsync();
                _logger.LogTrace("Authentication Header has been reset");

                // Retry the REST method
                results = await loadFolderApiCallAsync(url);
            }

            return results!;
        }
    }
}