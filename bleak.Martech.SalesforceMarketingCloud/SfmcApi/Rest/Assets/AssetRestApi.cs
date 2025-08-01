using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.Authentication;
using bleak.Martech.SalesforceMarketingCloud.Configuration;
using bleak.Martech.SalesforceMarketingCloud.Models.SfmcDtos;
using bleak.Martech.SalesforceMarketingCloud.Models.Pocos;
using bleak.Martech.SalesforceMarketingCloud.Rest;
using Microsoft.Extensions.Logging;
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
            SfmcConnectionConfiguration sfmcConnectionConfiguration,
            ILogger<AssetRestApi> logger
            ) :
            base(
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


        string _baseUrl => $"https://{_authRepository.Subdomain}.rest.marketingcloudapis.com/asset/v1/content/assets";

        public async Task<List<AssetPoco>> GetAssetsAsync(int folderId)
        {
            _logger.LogTrace("GetAssetsAsync() invoked");
            int page = 1;
            int currentPageSize = 0;
            var assets = new List<AssetPoco>();

            do
            {
                _logger.LogTrace($"Executing GetAssetsAsync() page: {page}");
                string url = $"{_baseUrl}?$page={page}&$pagesize={_sfmcConnectionConfiguration.PageSize}&$orderBy=name&$filter=category.id eq {folderId}";

                var loadedAssets = (await LoadAssetsAsync(
                    url: url,
                    page: page))
                    .ToPocoList();

                assets.AddRange(loadedAssets);
                currentPageSize = loadedAssets.Count;

                if (_sfmcConnectionConfiguration.PageSize == currentPageSize)
                {
                    _logger.LogInformation($"LoadAssetsAsync() returned currentPageSize: {loadedAssets.Count}. So far {assets.Count} assets have been loaded. Moving onto page {page + 1}.");
                    page++;
                }
                else
                {
                    _logger.LogInformation($"LoadAssetsAsync() returned currentPageSize: {loadedAssets.Count}. Loaded {assets.Count} assets total. No more pages to load.");
                    break; // exit loop if current page size < page size
                }
            }
            while (true);

            return assets;
        }

        public List<AssetPoco> GetAssets(int folderId)
        {
            try
            {
                return GetAssetsAsync(folderId).GetAwaiter().GetResult();
            }
            catch (AggregateException ae)
            {
                throw ae.InnerException ?? ae;
            }
        }

        public async Task<AssetPoco> GetAssetAsync
        (
            int? assetId = null,
            string? customerKey = null,
            string? name = null
        )
        {
            _logger.LogTrace("GetAssetAsync() invoked");
            int page = 1;
            var assets = new List<AssetPoco>();

            // input validation
            if (assetId == null && string.IsNullOrEmpty(customerKey) && string.IsNullOrEmpty(name))
            {
                _logger.LogError("GetAssetAsync() requires at least one of assetId, customerKey, or name to be provided.");
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
                    _logger.LogError("GetAssetAsync() requires at least one of assetId, customerKey, or name to be provided.");
                    throw new ArgumentException("At least one of assetId, customerKey, or name must be provided.");
                case 1:
                    _logger.LogTrace($"GetAssetAsync() called with a single parameter. assetId={assetId}, customerKey={customerKey}, name={name}.");
                    break;
                case > 1:
                    _logger.LogInformation($"GetAssetAsync() requires only one of assetId {assetId}, customerKey {customerKey}, or name {name} to be provided, but multiple were specified. Preferred Id, Key, Name, in that order.");
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
                _logger.LogError("Failed to construct a valid URL for GetAssetAsync.");
                throw new ArgumentException("A valid URL could not be constructed for GetAssetAsync.");
            }

            var loadedAssets = (await LoadAssetsAsync(
                url: url,
                page: page))
                .ToPocoList();

            assets.AddRange(loadedAssets);

            switch (loadedAssets.Count)
            {
                case 0:
                    _logger.LogError($"No assets found with ID {assetId}");
                    throw new Exception($"Asset with ID {assetId} not found");
                case 1:
                    _logger.LogTrace($"GetAssetAsync by assetId={assetId} returned exactly 1.");
                    break;
                default:
                    _logger.LogInformation($"GetAssetAsync by assetId={assetId} unexpectedly returned {loadedAssets.Count}. Only the first one will be returned.");
                    break;
            }

            var asset = loadedAssets.FirstOrDefault();
            return asset ?? throw new Exception($"Asset with ID {assetId} not found");
        }

        public AssetPoco GetAsset(int? assetId = null, string? customerKey = null, string? name = null)
        {
            try
            {
                return GetAssetAsync(assetId, customerKey, name).GetAwaiter().GetResult();
            }
            catch (AggregateException ae)
            {
                throw ae.InnerException ?? ae;
            }
        }


        private List<SfmcAsset> LoadAssets(string url, int page)
        {
            try
            {
                return LoadAssetsAsync(url, page).GetAwaiter().GetResult();
            }
            catch (AggregateException ae)
            {
                throw ae.InnerException ?? ae;
            }
        }

        private async Task<List<SfmcAsset>> LoadAssetsAsync(string url, int page)
        {
            _logger.LogInformation($"LoadAsset({url}, {page}) invoked");
            var retval = new List<SfmcAsset>();

            try
            {
                _logger.LogInformation($"Loading Asset Page #{page} with URL: {url}");

                var results = await ExecuteRestMethodWithRetryAsync(
                    apiCallAsync: LoadFolderApiCallAsync,
                    url: url,
                    authenticationError: "401",
                    resolveAuthenticationAsync: _authRepository.ResolveAuthenticationAsync
                );

                _logger.LogInformation($"results.Value = {results?.Results}");

                if (results?.Error != null)
                {
                    _logger.LogError($"results.Error = {results.Error}");
                }

                if (results?.Results?.items != null)
                {
                    retval.AddRange(results.Results.items);
                }

                _logger.LogInformation($"Current Page had {retval.Count} records in page {page}");
                return retval;
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message}");
                throw;
            }
        }

        private RestResults<SfmcRestWrapper<SfmcAsset>, string> LoadFolderApiCall(string url)
        {
            try
            {
                return LoadFolderApiCallAsync(url).GetAwaiter().GetResult();
            }
            catch (AggregateException ae)
            {
                throw ae.InnerException ?? ae;
            }
        }

        private async Task<RestResults<SfmcRestWrapper<SfmcAsset>, string>> LoadFolderApiCallAsync(string url)
        {
            _logger.LogTrace($"Attempting to {verb} to {url} with accessToken: {_authRepository.Token.access_token}");

            SetAuthHeader();

            var results = await _restManagerAsync.ExecuteRestMethodAsync<SfmcRestWrapper<SfmcAsset>, string>(
                uri: new Uri(url),
                verb: verb,
                headers: _headers
            );

            return results;
        }


        #region Candidate to move to BaseRestApi?




        private RestResults<SfmcRestWrapper<SfmcAsset>, string> ExecuteRestMethodWithRetry(
            Func<string, RestResults<SfmcRestWrapper<SfmcAsset>, string>> apiCall,
            string url,
            string authenticationError,
            Action resolveAuthentication)
        {
            try
            {
                return ExecuteRestMethodWithRetryAsync(
                    apiCallAsync: url => Task.FromResult(apiCall(url)),
                    url: url,
                    authenticationError: authenticationError,
                    resolveAuthenticationAsync: () =>
                    {
                        resolveAuthentication();
                        return Task.CompletedTask;
                    }
                ).GetAwaiter().GetResult();
            }
            catch (AggregateException ae)
            {
                throw ae.InnerException ?? ae;
            }
        }

        
        private async Task<RestResults<SfmcRestWrapper<SfmcAsset>, string>> ExecuteRestMethodWithRetryAsync(
            Func<string, Task<RestResults<SfmcRestWrapper<SfmcAsset>, string>>> apiCallAsync,
            string url,
            string authenticationError,
            Func<Task> resolveAuthenticationAsync)
        {
            var results = await apiCallAsync(url);

            // Check if an error occurred and it matches the specified errorText
            if (results != null && results.UnhandledError != null && results.UnhandledError.Contains(authenticationError))
            {
                _logger.LogInformation($"Unauthenticated: {results.UnhandledError}");

                // Resolve authentication
                await resolveAuthenticationAsync();
                _logger.LogInformation($"Authentication Header has been reset");

                // Retry the REST method
                results = await apiCallAsync(url);
            }

            return results!;
        }


        #endregion Candidate to move to BaseRestApi?

    }
}