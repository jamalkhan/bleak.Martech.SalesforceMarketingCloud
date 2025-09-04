using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Configuration;
using bleak.Martech.SalesforceMarketingCloud.Authentication;
using bleak.Martech.SalesforceMarketingCloud.Models;
using bleak.Martech.SalesforceMarketingCloud.Models.SfmcDtos;
using System.Diagnostics;
using System.ServiceModel;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace bleak.Martech.SalesforceMarketingCloud.ConsoleApp.ConsoleApps;

public class LoadFolders
{
    IRestClientAsync _restClientAsync;
    IAuthRepository _authRepository;
    public LoadFolders(IRestClientAsync restClientAsync, IAuthRepository authRepository)
    {
        _restClientAsync = restClientAsync;
        _authRepository = authRepository;
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
            if (AppConfiguration.Instance.Debug) { Console.WriteLine($"Loading Folder Page #{page}"); }
            
            RestResults<SfmcRestWrapper<SfmcFolder>, string> results;
            string url = $"https://{AppConfiguration.Instance.Subdomain}.rest.marketingcloudapis.com/asset/v1/content/categories?$page={page}&$pagesize={AppConfiguration.Instance.PageSize}";

            results = await ExecuteRestMethodWithRetryAsync(
                loadFolderApiCallAsync: LoadFolderApiCallAsync,
                url: url,
                authenticationError: "401"
            );

            if (AppConfiguration.Instance.Debug) Console.WriteLine($"results.Value = {results?.Results}");
            if (results?.Error != null) Console.WriteLine($"results.Error = {results.Error}");

            currentPageSize = results!.Results.items.Count();
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

    private async Task<RestResults<SfmcRestWrapper<SfmcFolder>, string>> LoadFolderApiCallAsync(
        string url
    )
    {
        var token = await _authRepository.GetTokenAsync();
        // string opens = await GetTrackingData($"{baseUri}/messageDefinitionSends/key:{definitionKey}/tracking/opens", accessToken);
        // string clicks = await GetTrackingData($"{baseUri}/messageDefinitionSends/key:{definitionKey}/tracking/clicks", accessToken);
        // string bounces = await GetTrackingData($"{baseUri}/messageDefinitionSends/key:{definitionKey}/tracking/bounces", accessToken);
        // string sends = await GetTrackingData($"{baseUri}/messageDefinitionSends/key:{definitionKey}/tracking", accessToken);

        if (AppConfiguration.Instance.Debug) { Console.WriteLine($"Attempting to {verb} to {url} with accessToken: {token.access_token}"); }

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
            Console.WriteLine($"Unauthenticated: {results.UnhandledError}");

            // Retry the REST method
            results = await loadFolderApiCallAsync(url).ConfigureAwait(false);

            Console.WriteLine("Press Enter to Continue");
            // You may want to remove or replace this for non-blocking UI scenarios
            Console.ReadLine();
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