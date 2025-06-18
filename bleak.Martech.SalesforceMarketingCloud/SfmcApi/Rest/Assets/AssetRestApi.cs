using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.Authentication;
using bleak.Martech.SalesforceMarketingCloud.Configuration;
using bleak.Martech.SalesforceMarketingCloud.Models.SfmcDtos;
using bleak.Martech.SalesforceMarketingCloud.Models.Pocos;
using bleak.Martech.SalesforceMarketingCloud.Rest;
using Microsoft.Extensions.Logging;

namespace bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Sfmc.Rest.Assets
{
    public class AssetRestApi
    : BaseRestApi
    , IAssetRestApi
    {
        private readonly ILogger<AssetRestApi> _logger;

        private HttpVerbs verb = HttpVerbs.GET;

        public AssetRestApi(
            IAuthRepository authRepository,
            SfmcConnectionConfiguration config,
            ILogger<AssetRestApi> logger
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

        public async Task<List<AssetPoco>> GetAssetsAsync(int folderId)
        {
            _logger.LogInformation("GetAssetsAsync() invoked");
            return await Task.Run(() => GetAssets(folderId));
        }

        public List<AssetPoco> GetAssets(int folderId)
        {
            _logger.LogTrace("GetAssets() invoked");
            int page = 1;
            int currentPageSize = 0;

            var assets = new List<AssetPoco>();
            do
            {
                _logger.LogTrace($"Executing GetAssets() page: {page}");
                assets.AddRange(
                    LoadAssets(folderId, page)
                        .ToPocoList()
                    );
                page++;
                _logger.LogInformation($"LoadAssets() returned currentPageSize: {currentPageSize} and moving onto page {page}.");
            }
            while (_sfmcConnectionConfiguration.PageSize == currentPageSize);

            _logger.LogError("Error Loading Folders");
            throw new Exception("Error Loading Folders");
        }

        private List<SfmcAsset> LoadAssets(int folderId, int page)
        {
            var retval = new List<SfmcAsset>();
            int currentPageSize;
            try
            {
                RestResults<SfmcRestWrapper<SfmcAsset>, string> results;
                // /asset/v1/content/assets
                string url = $"https://{_authRepository.Subdomain}.rest.marketingcloudapis.com/asset/v1/content/assets?$page={page}&$pagesize={_sfmcConnectionConfiguration.PageSize}&$orderBy=name&$filter=category.id eq {folderId}";
                _logger.LogInformation($"Loading Folder Page #{page} with URL: {url}");
                results = ExecuteRestMethodWithRetry(
                    apiCall: LoadFolderApiCall,
                    url: url,
                    authenticationError: "401",
                    resolveAuthentication: _authRepository.ResolveAuthentication
                );

                _logger.LogTrace($"results.Value = {results?.Results}");
                _logger.LogError($"results.Error = {results?.Error}");

                currentPageSize = results!.Results.items.Count();
                retval.AddRange(results.Results.items);
                _logger.LogInformation($"Current Page had {currentPageSize} records. There are now {retval.Count()} Total Folders Identified.");
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"{ex.Message}");
                throw;
            }

            return retval;
        }

        private RestResults<SfmcRestWrapper<SfmcAsset>, string> LoadFolderApiCall(
            string url
        )
        {
            _logger.LogTrace($"Attempting to {verb} to {url} with accessToken: {_authRepository.Token.access_token}");
            SetAuthHeader();
            var results = _restManager.ExecuteRestMethod<SfmcRestWrapper<SfmcAsset>, string>(
                uri: new Uri(url),
                verb: verb,
                headers: _headers
                );
            return results!;
        }

        #region Candidate to move to BaseRestApi?
        private RestResults<SfmcRestWrapper<SfmcAsset>, string> ExecuteRestMethodWithRetry(
            Func<string, RestResults<SfmcRestWrapper<SfmcAsset>, string>> apiCall,
            string url,
            string authenticationError,
            Action resolveAuthentication
            )
        {
            var results = apiCall(url);

            // Check if an error occurred and it matches the specified errorText
            if (results != null && results.UnhandledError != null && results.UnhandledError.Contains(authenticationError))
            {
                _logger.LogInformation($"Unauthenticated: {results.UnhandledError}");

                // Resolve authentication
                resolveAuthentication();
                _logger.LogInformation($"Authentication Header has been reset");

                // Retry the REST method
                results = apiCall(url);
            }

            return results!;
        }
        #endregion Candidate to move to BaseRestApi?
    }
}