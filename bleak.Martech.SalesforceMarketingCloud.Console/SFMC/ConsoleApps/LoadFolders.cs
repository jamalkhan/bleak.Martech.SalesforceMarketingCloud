using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Configuration;
using bleak.Martech.SalesforceMarketingCloud.Authentication;
using bleak.Martech.SalesforceMarketingCloud.Models;
using bleak.Martech.SalesforceMarketingCloud.Models.Sfmc;
using System.Diagnostics;
using System.ServiceModel;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace bleak.Martech.SalesforceMarketingCloud.ConsoleApp.ConsoleApps;

public class LoadFolders
{
    IRestClientAsync _restClientAsync;
    IAuthRepository _authRepository;
    private readonly ILogger _logger;
    public LoadFolders(IRestClientAsync restClientAsync, IAuthRepository authRepository, ILogger logger)
    {
        _restClientAsync = restClientAsync;
        _authRepository = authRepository;
        _logger = logger;
    }

    private HttpVerbs verb = HttpVerbs.GET;
    private List<Header> headers = new List<Header>
    {
        new Header() { Name = "Content-Type", Value = "application/json" },
    };

    public async Task<List<FolderObject>> GetFolderTreeAsync()
    {
        int page = 1;
        int currentPageSize = 0;
        
        var sfmcFolders = new List<SfmcFolder>();
        do
        {
            currentPageSize = await LoadFolderAsync(page, sfmcFolders);
            page++;
        }
        while (AppConfiguration.Instance.PageSize == currentPageSize);

        if (sfmcFolders.Any())
        {
            return BuildFolderTree(sfmcFolders);
        }

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

            // TODO: Reimplement this
            // GetAssetsByFolder(folderObject);
            // AddChildren(folderObject, sfmcFolders);
            retval.Add(folderObject);
        }
        return retval;
    }

    private async Task<int> LoadFolderAsync(int page, List<SfmcFolder> sfmcFolders)
    {
        int currentPageSize;
        try
        {
            _logger.LogDebug("Loading folder page {PageNumber}.", page);
            
            RestResults<SfmcRestWrapper<SfmcFolder>, string> results;
            string url = $"{bleak.Martech.SalesforceMarketingCloud.Configuration.SfmcEndpointUrls.GetRestEndpoint(AppConfiguration.Instance.Subdomain, "/asset/v1/content/categories", AppConfiguration.Instance.RestBaseUrl)}?$page={page}&$pagesize={AppConfiguration.Instance.PageSize}";

            results = await ExecuteRestMethodWithRetryAsync(
                loadFolderApiCallAsync: LoadFolderApiCallAsync,
                url: url,
                authenticationError: "401"
            );

            _logger.LogTrace("Folder page {PageNumber} completed. HasResults={HasResults}", page, results?.Results != null);
            if (results?.Error != null) _logger.LogWarning("Folder page {PageNumber} returned an error. Error={Error}", page, results.Error);

            currentPageSize = results!.Results.items.Count();
            sfmcFolders.AddRange(results.Results.items);
            _logger.LogDebug("Folder page {PageNumber} loaded {PageCount} records. AggregateCount={AggregateCount}", page, currentPageSize, sfmcFolders.Count);

            if (AppConfiguration.Instance.PageSize == currentPageSize)
            {
                _logger.LogTrace("More folder pages remain after page {PageNumber}.", page);
            }

        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Folder page load failed for page {PageNumber}.", page);
            throw;
        }

        return currentPageSize;
    }

    private async Task<RestResults<SfmcRestWrapper<SfmcFolder>, string>> LoadFolderApiCallAsync(
        string url
    )
    {
        var token = await _authRepository.GetTokenAsync();
        // string opens = await GetTrackingData($"{baseUri}/messageDefinitionSends/key:{definitionKey}/tracking/opens", accessToken);
        // string clicks = await GetTrackingData($"{baseUri}/messageDefinitionSends/key:{definitionKey}/tracking/clicks", accessToken);
        // string bounces = await GetTrackingData($"{baseUri}/messageDefinitionSends/key:{definitionKey}/tracking/bounces", accessToken);
        // string sends = await GetTrackingData($"{baseUri}/messageDefinitionSends/key:{definitionKey}/tracking", accessToken);

        _logger.LogTrace("Attempting folder request. Verb={Verb}, Url={Url}", verb, url);

        var headersWithAuth = await SetAuthHeaderAsync(headers);

        var results = await _restClientAsync.ExecuteRestMethodAsync<SfmcRestWrapper<SfmcFolder>, string>(
            uri: new Uri(url),
            verb: verb,
            headers: headersWithAuth
            );

        return results!;
    }

    private async Task<RestResults<SfmcRestWrapper<SfmcFolder>, string>> ExecuteRestMethodWithRetryAsync(
        Func<string, Task<RestResults<SfmcRestWrapper<SfmcFolder>, string>>> loadFolderApiCallAsync,
        string url,
        string authenticationError
    )
    {
        var results = await loadFolderApiCallAsync(url).ConfigureAwait(false);

        // Check if an error occurred and it matches the specified errorText
        if (results != null && results.UnhandledError != null &&
            results.UnhandledError.Contains(authenticationError))
        {
            _logger.LogWarning("Folder request returned unauthenticated response. Error={Error}", results.UnhandledError);

            // Retry the REST method
            results = await loadFolderApiCallAsync(url).ConfigureAwait(false);
        }

        return results!;
    }


    private async Task<List<Header>> SetAuthHeaderAsync(List<Header> headers)
    {
        var token = await _authRepository.GetTokenAsync();
        var headersWithAuth = new List<Header>();

        foreach (var header in headers)
        {
            headersWithAuth.Add(new Header() { Name = header.Name, Value = header.Value });
        }

        headersWithAuth.Add(
            new Header() { Name = "Authorization", Value = $"Bearer {token.access_token}" }
        );

        return headersWithAuth;
    }
}
