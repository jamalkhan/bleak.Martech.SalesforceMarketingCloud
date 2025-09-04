using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.Authentication;
using bleak.Martech.SalesforceMarketingCloud.Configuration;
using bleak.Martech.SalesforceMarketingCloud.Models.SfmcDtos;
using bleak.Martech.SalesforceMarketingCloud.Models.Pocos;
using bleak.Martech.SalesforceMarketingCloud.Rest;
using Microsoft.Extensions.Logging;
using bleak.Martech.SalesforceMarketingCloud.Models.Helpers;
using bleak.Martech.SalesforceMarketingCloud.Api;
using System.Text;
using System.Linq;

namespace bleak.Martech.SalesforceMarketingCloud.Sfmc.Rest.Assets
{
    public class AssetRestApi
    : BaseRestApi<AssetRestApi>
    , IAssetRestApi
    {
        private HttpVerbs verb = HttpVerbs.GET;

        public AssetRestApi(
            IRestClientAsync restClientAsync,
            IAuthRepository authRepository,
            SfmcConnectionConfiguration config,
            ILogger<AssetRestApi> logger
            ) :
            base(
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


        string _baseUrl => $"https://{_authRepository.Subdomain}.rest.marketingcloudapis.com/asset/v1/content/assets";

        public async Task<IEnumerable<AssetPoco>> GetAssetsAsync(int folderId)
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
                    _logger.LogTrace($"LoadAssetsAsync() returned currentPageSize: {loadedAssets.Count}. So far {assets.Count} assets have been loaded. Moving onto page {page + 1}.");
                    page++;
                }
                else
                {
                    _logger.LogTrace($"LoadAssetsAsync() returned currentPageSize: {loadedAssets.Count}. Loaded {assets.Count} assets total. No more pages to load.");
                    break; // exit loop if current page size < page size
                }
            }
            while (true);

            return assets;
        }



        public async Task<IEnumerable<AssetPoco>> SearchAssetsAsync
        (
            string searchTerm, 
            int? folderId = null
        )
        {
            

            _logger.LogTrace("SearchAssetsAsync() invoked");

            var assets = new List<AssetPoco>();
            assets.AddRange(await SearchByAsync("name", searchTerm));
            assets.AddRange(await SearchByAsync("customerKey", searchTerm));
            return assets.OrderBy(a => a.Name);
        }

        private async Task<List<AssetPoco>> SearchByAsync(string key, string searchTerm)
        {
            var assets = new List<AssetPoco>();
            int page = 1;
            int currentPageSize = 0;
            do
            {
                _logger.LogTrace($"Executing SearchAssetsAsync() page: {page}");
                var url = $"{_baseUrl}?$page={page}&$pagesize={_sfmcConnectionConfiguration.PageSize}&$filter={key} like '%{searchTerm}%'";

                //sbUrl.Append("%' or customerKey like '%");

                var namedAssets = (await LoadAssetsAsync(
                    url: url,
                    page: page))
                    .ToPocoList();

                assets.AddRange(namedAssets);
                currentPageSize = namedAssets.Count;

                if (_sfmcConnectionConfiguration.PageSize == currentPageSize)
                {
                    _logger.LogTrace($"LoadAssetsAsync() returned currentPageSize: {namedAssets.Count}. So far {assets.Count} assets have been loaded. Moving onto page {page + 1}.");
                    page++;
                }
                else
                {
                    _logger.LogTrace($"LoadAssetsAsync() returned currentPageSize: {namedAssets.Count}. Loaded {assets.Count} assets total. No more pages to load.");
                    break; // exit loop if current page size < page size
                }
            }
            while (true);
            return assets;
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
                    _logger.LogTrace($"GetAssetAsync() requires only one of assetId {assetId}, customerKey {customerKey}, or name {name} to be provided, but multiple were specified. Preferred Id, Key, Name, in that order.");
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
                    _logger.LogTrace($"GetAssetAsync by assetId={assetId} unexpectedly returned {loadedAssets.Count}. Only the first one will be returned.");
                    break;
            }

            var asset = loadedAssets.FirstOrDefault();
            return asset ?? throw new Exception($"Asset with ID {assetId} not found");
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
            _logger.LogTrace($"LoadAsset({url}, {page}) invoked");
            var retval = new List<SfmcAsset>();

            try
            {
                _logger.LogTrace($"Loading Asset Page #{page} with URL: {url}");

                var results = await ExecuteRestMethodWithRetryAsync(
                    apiCallAsync: LoadFolderApiCallAsync,
                    url: url,
                    authenticationError: "401"
                );

                _logger.LogTrace($"results.Value = {results?.Results}");

                if (results?.Error != null)
                {
                    _logger.LogError($"results.Error = {results.Error}");
                }

                if (results?.Results?.items != null)
                {
                    retval.AddRange(results.Results.items);
                }

                _logger.LogTrace($"Current Page had {retval.Count} records in page {page}");
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
            _logger.LogTrace($"Attempting to {verb} to {url}");

            await SetAuthHeaderAsync();

            var results = await _restClientAsync.ExecuteRestMethodAsync<SfmcRestWrapper<SfmcAsset>, string>
            (
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
                return ExecuteRestMethodWithRetryAsync
                (
                    apiCallAsync: url => Task.FromResult(apiCall(url)),
                    url: url,
                    authenticationError: authenticationError
                ).GetAwaiter().GetResult();
            }
            catch (AggregateException ae)
            {
                throw ae.InnerException ?? ae;
            }
        }

        
        private async Task<RestResults<SfmcRestWrapper<SfmcAsset>, string>> ExecuteRestMethodWithRetryAsync
        (
            Func<string, Task<RestResults<SfmcRestWrapper<SfmcAsset>, string>>> apiCallAsync,
            string url,
            string authenticationError
        )
        {
            var results = await apiCallAsync(url);

            // Check if an error occurred and it matches the specified errorText
            if (results != null && results.UnhandledError != null && results.UnhandledError.Contains(authenticationError))
            {
                _logger.LogTrace($"Unauthenticated: {results.UnhandledError}");

                // Retry the REST method
                results = await apiCallAsync(url);
            }

            return results!;
        }


        #endregion Candidate to move to BaseRestApi?

    }
}