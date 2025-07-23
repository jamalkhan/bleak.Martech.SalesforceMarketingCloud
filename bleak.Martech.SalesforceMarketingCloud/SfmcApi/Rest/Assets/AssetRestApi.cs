using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.Authentication;
using bleak.Martech.SalesforceMarketingCloud.Configuration;
using bleak.Martech.SalesforceMarketingCloud.Models.SfmcDtos;
using bleak.Martech.SalesforceMarketingCloud.Models.Pocos;
using bleak.Martech.SalesforceMarketingCloud.Rest;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using bleak.Martech.SalesforceMarketingCloud.Models.Helpers;

namespace bleak.Martech.SalesforceMarketingCloud.Sfmc.Rest.Assets
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

        string _baseUrl => $"https://{_authRepository.Subdomain}.rest.marketingcloudapis.com/asset/v1/content/assets";
        public List<AssetPoco> GetAssets(int folderId)
        {
            _logger.LogTrace("GetAssets() invoked");
            int page = 1;
            int currentPageSize = 0;
            var assets = new List<AssetPoco>();
            do
            {
                _logger.LogTrace($"Executing GetAssets() page: {page}");
                string url = $"{_baseUrl}?$page={page}&$pagesize={_sfmcConnectionConfiguration.PageSize}&$orderBy=name&$filter=category.id eq {folderId}";
                var loadedAssets = LoadAssets(
                    url: url,
                    page: page)
                    .ToPocoList();
                assets.AddRange(loadedAssets);
                currentPageSize = loadedAssets.Count;
                if (_sfmcConnectionConfiguration.PageSize == currentPageSize)
                {
                    _logger.LogInformation($"LoadAssets() returned currentPageSize: {loadedAssets.Count}. So far {assets.Count} assets have been loaded. Moving onto page {page}.");
                    page++;
                }
                else
                {
                    _logger.LogInformation($"LoadAssets() returned currentPageSize: {loadedAssets.Count}. Loaded {assets.Count} assets total. No more pages to load.");
                    break; // exit the loop if the current page size is less than the configured page size
                }
            }
            while (true);
            return assets;
        }

        public async Task<AssetPoco> GetAssetAsync(int? assetId = null, string? customerKey = null, string? name = null)
        {
            _logger.LogInformation("GetAssetAsync() invoked");
            return await Task.Run(() => GetAsset(assetId: assetId, customerKey: customerKey, name: name ));
        }
        public AssetPoco GetAsset(int? assetId = null, string? customerKey = null, string? name = null)
        {
            _logger.LogTrace("GetAsset() invoked");
            int page = 1;
            var assets = new List<AssetPoco>();

            // input validation
            if (assetId == null && string.IsNullOrEmpty(customerKey) && string.IsNullOrEmpty(name))
            {
                _logger.LogError("GetAsset() requires at least one of assetId, customerKey, or name to be provided.");
                throw new ArgumentException("At least one of assetId, customerKey, or name must be provided.");
            }

            // Ensure only one of assetId, customerKey, or name is provided
            int providedCount = 0;
            if (assetId != null) providedCount++;
            if (!string.IsNullOrEmpty(customerKey)) providedCount++;
            if (!string.IsNullOrEmpty(name)) providedCount++;
            switch (providedCount)
            {
                case 0:
                    _logger.LogError("GetAsset() requires at least one of assetId, customerKey, or name to be provided.");
                    throw new ArgumentException("At least one of assetId, customerKey, or name must be provided.");
                case 1:
                    _logger.LogTrace($"GetAsset() called with a single parameter. assetId={assetId}, customerKey={customerKey}, name={name}.");
                    break;
                case > 1:
                    _logger.LogError("GetAsset() requires only one of assetId, customerKey, or name to be provided, but multiple were specified.");
                    throw new ArgumentException("Only one of assetId, customerKey, or name can be specified at a time.");
                default:
                    break;
            }

            string url = string.Empty;
            if (assetId != null)
            {
                url = $"{_baseUrl}?$filter=id eq {assetId}";
            }
            else if (!string.IsNullOrEmpty(customerKey))
            {
                url = $"{_baseUrl}?$filter=customerKey eq '{customerKey}'";
            }
            else if (!string.IsNullOrEmpty(name))
            {
                url = $"{_baseUrl}?$filter=name eq '{name}'";
            }

            if (string.IsNullOrEmpty(url))
            {
                _logger.LogError("Failed to construct a valid URL for GetAsset.");
                throw new ArgumentException("A valid URL could not be constructed for GetAsset.");
            }

            var loadedAssets = LoadAssets(
                url: url,
                page: page)
                .ToPocoList();
            assets.AddRange(loadedAssets);

            switch (loadedAssets.Count)
            {
                case 0:
                    _logger.LogError($"No assets found with ID {assetId}");
                    throw new Exception($"Asset with ID {assetId} not found");
                case 1:
                    _logger.LogTrace($"GetAsset by assetId={assetId} returned exactly 1.");
                    break;
                default:
                    _logger.LogInformation($"GetAsset by assetId={assetId} unexpectedly returned {loadedAssets.Count}. Only the first one will be returned.");
                    break;
            }
            var asset = loadedAssets.FirstOrDefault();
            return asset
                ?? throw new Exception($"Asset with ID {assetId} not found");
            ;
        }

        private List<SfmcAsset> LoadAssets(string url, int page)
        {
            var retval = new List<SfmcAsset>();
            try
            {
                RestResults<SfmcRestWrapper<SfmcAsset>, string> results;

                _logger.LogInformation($"Loading Folder Page #{page} with URL: {url}");
                results = ExecuteRestMethodWithRetry(
                    apiCall: LoadFolderApiCall,
                    url: url,
                    authenticationError: "401",
                    resolveAuthentication: _authRepository.ResolveAuthentication
                );

                _logger.LogTrace($"results.Value = {results?.Results}");
                if (results?.Error != null) _logger.LogError($"results.Error = {results?.Error}");

                if (results?.Results?.items != null)
                {
                    retval.AddRange(results.Results.items);
                }
                _logger.LogInformation($"Current Page had {retval.Count()} records in page {page}");
                return retval;
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"{ex.Message}");
                throw;
            }
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