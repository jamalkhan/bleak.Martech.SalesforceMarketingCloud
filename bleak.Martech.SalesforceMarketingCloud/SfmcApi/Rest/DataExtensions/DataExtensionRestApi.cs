using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.Authentication;
using bleak.Martech.SalesforceMarketingCloud.Models.SfmcDtos;
using bleak.Martech.SalesforceMarketingCloud.Configuration;
using bleak.Martech.SalesforceMarketingCloud.Rest;
using bleak.Martech.SalesforceMarketingCloud.Api;
using bleak.Martech.SalesforceMarketingCloud.Fileops;
using bleak.Martech.SalesforceMarketingCloud.Models.Helpers;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace bleak.Martech.SalesforceMarketingCloud.Sfmc.Rest.DataExtensions;

public class DataExtensionRestApi
:
    BaseRestApi
    <
        DataExtensionRestApi
    >,
    IDataExtensionRestApi
{
    public DataExtensionRestApi
    (
        IRestClientAsync restClientAsync,
        IAuthRepository authRepository,
        SfmcConnectionConfiguration config,
        ILogger<DataExtensionRestApi> logger
    )
        : base
        (
            restClientAsync: restClientAsync,
            authRepository: authRepository,
            config: config,
            logger: logger
        )
    {
    }

    private HttpVerbs verb = HttpVerbs.GET;
    //"https://{{et_subdomain}}.rest.marketingcloudapis.com/data/v1/customobjectdata/key/Person_NA/rowset";


    public async Task<long> DownloadDataExtensionAsync
        (
        string dataExtensionCustomerKey,
        IFileWriter fileWriter,
        string fileName
        )
    {
        int currentPageSize;
        long totalRecords = 0;
        try
        {
            _logger.LogInformation($"writing file to {fileName}");

            Directory.CreateDirectory(Path.GetDirectoryName(fileName)!);

            string baseUrl = $"https://{_authRepository.Subdomain}.rest.marketingcloudapis.com/data";
            string url = $"{baseUrl}/v1/customobjectdata/key/{dataExtensionCustomerKey}/rowset?$page=1&$pageSize=2500";
            do
            {
                _logger.LogInformation($"Downloading Data Extension[{dataExtensionCustomerKey}] from  {url}");
                var results = await LoadApiWithRetryAsync<DataExtensionDataDto>(
                    loadApiCallAsync: LoadApiCallAsync,
                    url: url,
                    authenticationError: "401",
                    resolveAuthenticationAsync: _authRepository.ResolveAuthenticationAsync
                );
                
                if (results?.Error != null)
                {
                    throw new Exception($"Error: {results.Error}");
                }

                if (results?.Results?.items == null)
                {
                    throw new Exception("API returned no results.");
                }

                currentPageSize = results!.Results!.items!.Count();
                totalRecords += currentPageSize;

                // add data to return value.
                fileWriter.WriteToFile(fileName, results.Results.ToDictionaryList());

                _logger.LogInformation($"Current Page had {currentPageSize} records. There are now {totalRecords} Records Identified.");

                if (results == null || results.Results == null || results.Results.links == null || results.Results.links == null || string.IsNullOrEmpty(results.Results.links.next))
                {
                    _logger.LogInformation($"No more pages to process. Exiting loop.");
                    break;
                }

                _logger.LogTrace($"Running Loop Again");
                url = $"{baseUrl}{results.Results.links.next}";
            } while (true);
        }
        catch (System.Exception ex)
        {
            _logger.LogError($"{ex.Message}");
            throw;
        }
        return totalRecords;
    }


    // TODO: Figure out how to Generic this and move down to BaseRestApi
    private async Task<RestResults<DataExtensionDataDto, string>> LoadApiCallAsync(
        string url
    )
    {
        _logger.LogInformation($"Attempting to {verb} to {url}");

        await SetAuthHeaderAsync();
        return await _restClientAsync.ExecuteRestMethodAsync<DataExtensionDataDto, string>(
            uri: new Uri(url),
            verb: verb,
            headers: _headers
            );
    }
}
