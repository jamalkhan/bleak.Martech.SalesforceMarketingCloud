using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.Authentication;
using bleak.Martech.SalesforceMarketingCloud.Models.SfmcDtos;
using bleak.Martech.SalesforceMarketingCloud.Models;
using bleak.Martech.SalesforceMarketingCloud.Configuration;
using bleak.Martech.SalesforceMarketingCloud.Rest;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace bleak.Martech.SalesforceMarketingCloud.Sfmc.Rest.Assets;

public class AssetFolderRestApi
: BaseRestApi<AssetFolderRestApi>
, IAssetFolderRestApi
{

    private HttpVerbs verb = HttpVerbs.GET;

    public AssetFolderRestApi(
        IRestClientAsync restClientAsync,
        IAuthRepository authRepository,
        SfmcConnectionConfiguration config,
        ILogger<AssetFolderRestApi> logger
        )
        : base(
            restClientAsync: restClientAsync,
            authRepository: authRepository,
            config: config,
            logger: logger
        )
    {
        
        if (config == null) config = new SfmcConnectionConfiguration();
        if (config.PageSize > 500)
        {
            base._sfmcConnectionConfiguration.PageSize = 500; // Set a reasonable default max page size
        }
    }

    public async Task<List<FolderObject>> GetFolderTreeAsync()
    {
        _logger.LogTrace("GetFolderTreeAsync() invoked");
        int page = 1;
        int currentPageSize = 0;

        var sfmcFolders = new List<SfmcFolder>();
        do
        {
            _logger.LogTrace($"Executing GetFolderTreeAsync() page: {page}");
            currentPageSize = await LoadFolderAsync(page, sfmcFolders);
            page++;
            _logger.LogTrace($"LoadFolderAsync() returned currentPageSize: {currentPageSize} and moving onto page {page}.");
        }
        while (_sfmcConnectionConfiguration.PageSize == currentPageSize);

        _logger.LogInformation($"LoadFolderAsync() loaded {sfmcFolders.Count()} Folders");
        if (sfmcFolders.Any())
        {
            return await BuildFolderTreeAsync(sfmcFolders);
        }
        _logger.LogError("Error Loading Folders");
        throw new Exception("Error Loading Folders");
    }

    private async Task<List<FolderObject>> BuildFolderTreeAsync(List<SfmcFolder> sfmcFolders)
    {
        const int root_folder = 0;

        // Find root folders
        var sfmcRoots = sfmcFolders.Where(f => f.parentId == root_folder).ToList();
        var retval = new List<FolderObject>();

        var tasks = sfmcRoots.Select(async sfmcRoot =>
        {
            var folderObject = sfmcRoot.ToFolderObject();
            folderObject.FullPath = "/";
            await AddChildrenAsync(folderObject, folderObject.FullPath, sfmcFolders).ConfigureAwait(false);
            _logger.LogTrace($"Added root folder: {folderObject.Name} (ID: {folderObject.Id}) with FullPath: {folderObject.FullPath} with {folderObject.SubFolders?.Count ?? 0} subfolders.");
            return folderObject;
        });

        retval = (await Task.WhenAll(tasks).ConfigureAwait(false)).ToList();

        return retval;
    }

    private async Task AddChildrenAsync(FolderObject parentFolder, string path, List<SfmcFolder> sfmcFolders)
    {
        var childFolders = sfmcFolders
            .Where(f => f.parentId == parentFolder.Id)
            .OrderBy(f => f.name)
            .ToList();

        if (!childFolders.Any())
        {
            _logger.LogTrace($"No child folders found for parent folder: {parentFolder.Name} (ID: {parentFolder.Id})");
            return;
        }

        _logger.LogTrace($"Found {childFolders.Count} child folders for parent folder: {parentFolder.Name} (ID: {parentFolder.Id})");
        parentFolder.SubFolders = new List<FolderObject>();

        var tasks = childFolders.Select(async childFolder =>
        {
            var childFolderObject = childFolder.ToFolderObject();
            childFolderObject.FullPath = path + childFolderObject.Name + "/";

            _logger.LogTrace($"Adding child folder: {childFolderObject.Name} (ID: {childFolderObject.Id}) to parent folder: {parentFolder.Name} (ID: {parentFolder.Id})");

            parentFolder.SubFolders.Add(childFolderObject);

            // Recursively add children to the child folder
            await AddChildrenAsync(
                parentFolder: childFolderObject,
                path: childFolderObject.FullPath,
                sfmcFolders: sfmcFolders
            ).ConfigureAwait(false);
        });

        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    private async Task<int> LoadFolderAsync(int page, List<SfmcFolder> sfmcFolders)
    {
        int currentPageSize;
        try
        {
            _logger.LogInformation($"Starting LoadFolderAsync for page {page}");
            RestResults<SfmcRestWrapper<SfmcFolder>, string> results;
            //asset/v1/content/categories
            string url = $"https://{_authRepository.Subdomain}.rest.marketingcloudapis.com/asset/v1/content/categories/?$page={page}&$pagesize={_sfmcConnectionConfiguration.PageSize}";
            _logger.LogInformation($"Loading Folder Page #{page} with URL: {url}");
            
            _logger.LogInformation("About to call ExecuteRestMethodAsyncWithRetry");
            results = await ExecuteRestMethodAsyncWithRetry(
                loadFolderApiCallAsync: LoadFolderApiCallAsync,
                url: url,
                authenticationError: "401",
                resolveAuthenticationAsync: _authRepository.ResolveAuthenticationAsync
            );
            _logger.LogInformation("ExecuteRestMethodAsyncWithRetry completed");

            _logger.LogTrace($"results.Value = {results?.Results}");
            if (results?.Error != null) _logger.LogError($"results.Error = {results?.Error}");

            currentPageSize = results!.Results.items.Count();
            sfmcFolders.AddRange(results.Results.items);
            _logger.LogInformation($"Current Page had {currentPageSize} records. There are now {sfmcFolders.Count()} Total Folders Identified.");

            if (_sfmcConnectionConfiguration.PageSize == currentPageSize)
            {
                _logger.LogTrace($"Running Loop Again");
            }
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, $"Error in LoadFolderAsync for page {page}: {ex.Message}");
            throw;
        }

        return currentPageSize;
    }

    private async Task<RestResults<SfmcRestWrapper<SfmcFolder>, string>> LoadFolderApiCallAsync
    (
        string url
    )
    {
        _logger.LogInformation($"Starting LoadFolderApiCallAsync for URL: {url}");
        _logger.LogInformation($"Access token available: {!string.IsNullOrEmpty(_authRepository.Token.access_token)}");

        SetAuthHeader();
        _logger.LogInformation("Auth header set, about to execute REST call");
        
        try
        {
            // Add timeout to prevent hanging
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            
            var results = await _restClientAsync.ExecuteRestMethodAsync<SfmcRestWrapper<SfmcFolder>, string>(
                uri: new Uri(url),
                verb: verb,
                headers: _headers
                ).WaitAsync(cts.Token);
            
            _logger.LogInformation("REST call completed");
            return results!;
        }
        catch (OperationCanceledException)
        {
            _logger.LogError("REST API call timed out after 30 seconds");
            throw new TimeoutException("REST API call timed out. Please check your connection and try again.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "REST API call failed");
            throw;
        }
    }

    private async Task<RestResults<SfmcRestWrapper<SfmcFolder>, string>> ExecuteRestMethodAsyncWithRetry(
        Func<string, Task<RestResults<SfmcRestWrapper<SfmcFolder>, string>>> loadFolderApiCallAsync,
        string url,
        string authenticationError,
        Func<Task> resolveAuthenticationAsync
        )
    {
        _logger.LogInformation("Starting ExecuteRestMethodAsyncWithRetry");
        var results = await loadFolderApiCallAsync(url);
        _logger.LogInformation("First API call completed");

        // Check if an error occurred and it matches the specified errorText
        if (results != null && results.UnhandledError != null && results.UnhandledError.Contains(authenticationError))
        {
            _logger.LogInformation($"Authentication error detected: {results.UnhandledError}");

            // Resolve authentication
            await resolveAuthenticationAsync();
            _logger.LogInformation("Authentication resolved, retrying API call");

            // Retry the REST method
            results = await loadFolderApiCallAsync(url);
            _logger.LogInformation("Retry API call completed");
        }

        _logger.LogInformation("ExecuteRestMethodAsyncWithRetry completed");
        return results!;
    }
}