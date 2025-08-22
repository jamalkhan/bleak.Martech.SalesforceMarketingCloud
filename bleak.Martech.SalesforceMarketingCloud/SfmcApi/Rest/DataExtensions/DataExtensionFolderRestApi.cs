using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.Authentication;
using bleak.Martech.SalesforceMarketingCloud.Models.SfmcDtos;
using bleak.Martech.SalesforceMarketingCloud.Models;
using bleak.Martech.SalesforceMarketingCloud.Configuration;
using bleak.Martech.SalesforceMarketingCloud.Rest;
using bleak.Martech.SalesforceMarketingCloud.Api;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace bleak.Martech.SalesforceMarketingCloud.Sfmc.Rest.DataExtensions;

public class DataExtensionFolderRestApi
:
    BaseRestApi
    <
        DataExtensionFolderRestApi
    >,
    IDataExtensionFolderRestApi
{
    private HttpVerbs verb = HttpVerbs.GET;

    public DataExtensionFolderRestApi(
        IRestClientAsync restClientAsync,
        IAuthRepository authRepository,
        SfmcConnectionConfiguration sfmcConnectionConfiguration,
        ILogger<DataExtensionFolderRestApi> logger
        )
        : base(
            restClientAsync: restClientAsync,
            authRepository: authRepository,
            config: sfmcConnectionConfiguration,
            logger: logger
        )
    {
    }

    public async Task<List<FolderObject>> GetFolderTreeAsync()
    {
        return await Task.Run(() => GetFolderTree());
    }

    public async Task<List<FolderObject>> GetFolderTree()
    {
        int page = 1;
        int currentPageSize = 0;

        var sfmcFolders = new List<SfmcFolder>();
        do
        {
            currentPageSize = await LoadFolderAsync(page, sfmcFolders);
            page++;
        }
        while (_sfmcConnectionConfiguration.PageSize == currentPageSize);

        if (sfmcFolders.Any())
        {
            return BuildFolderTree(sfmcFolders);
        }

        throw new Exception("Error Loading Data Extension Folders via Rest");

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
        try
        {
            _logger.LogInformation($"Loading Folder Page #{page}");


            ///legacy/v1/beta/object/
            string url = $"https://{_authRepository.Subdomain}.rest.marketingcloudapis.com/legacy/v1/beta/object/?$page={page}&$pagesize={_sfmcConnectionConfiguration.PageSize}";
            var results = await LoadApiWithRetryAsync<SfmcRestWrapper<SfmcFolder>>(
                loadApiCallAsync: LoadFolderApiCallAsync,
                url: url,
                authenticationError: "401",
                resolveAuthenticationAsync: _authRepository.ResolveAuthenticationAsync
            );

            _logger.LogInformation($"results.Value = {results?.Results}");
            if (results?.Error != null) _logger.LogError($"results.Error = {results.Error}");

            var currentPageSize = results!.Results.items.Count();
            sfmcFolders.AddRange(results.Results.items);
            _logger.LogInformation($"Current Page had {currentPageSize} records. There are now {sfmcFolders.Count()} Total Folders Identified.");

            return currentPageSize;
        }
        catch (Exception ex)
        {
            _logger.LogError($"{ex.Message}");
            throw;
        }
    }

    private async Task<RestResults<SfmcRestWrapper<SfmcFolder>, string>> LoadFolderApiCallAsync(
        string url
    )
    {
        _logger.LogInformation($"Attempting to {verb} to {url}");
        await SetAuthHeaderAsync();
        return await _restClientAsync.ExecuteRestMethodAsync<SfmcRestWrapper<SfmcFolder>, string>(
            uri: new Uri(url),
            verb: verb,
            headers: _headers
        );
    }
}
