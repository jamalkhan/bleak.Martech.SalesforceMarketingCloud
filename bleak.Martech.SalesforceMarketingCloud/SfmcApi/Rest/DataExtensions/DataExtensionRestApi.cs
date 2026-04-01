using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.Authentication;
using bleak.Martech.SalesforceMarketingCloud.Models.Sfmc;
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
        int currentPage = 1;
        try
        {
            _logger.LogInformation("Starting data extension download. CustomerKey={CustomerKey}, FileName={FileName}", dataExtensionCustomerKey, fileName);

            Directory.CreateDirectory(Path.GetDirectoryName(fileName)!);

            string baseUrl = Configuration.SfmcEndpointUrls.GetRestEndpoint(_authRepository.Subdomain, "/data", _sfmcConnectionConfiguration.RestBaseUrl);
            string url = $"{baseUrl}/v1/customobjectdata/key/{dataExtensionCustomerKey}/rowset?$page=1&$pageSize=2500";
            do
            {
                _logger.LogDebug("Downloading data extension page {PageNumber}. CustomerKey={CustomerKey}, Url={Url}", currentPage, dataExtensionCustomerKey, url);
                var results = await LoadApiWithRetryAsync<DataExtensionDataDto>(
                    loadApiCallAsync: LoadApiCallAsync,
                    url: url,
                    authenticationError: "401"
                );

                if (results?.Error != null)
                {
                    _logger.LogError("Data extension download returned API error. CustomerKey={CustomerKey}, Error={Error}", dataExtensionCustomerKey, results.Error);
                    throw new Exception($"Error: {results.Error}");
                }

                if (results?.Results?.items == null)
                {
                    _logger.LogWarning("Data extension download returned no items. CustomerKey={CustomerKey}, Page={PageNumber}", dataExtensionCustomerKey, currentPage);
                    throw new Exception("API returned no results.");
                }

                currentPageSize = results!.Results!.items!.Count();
                totalRecords += currentPageSize;

                // add data to return value.
                fileWriter.WriteToFile(fileName, results.Results.ToDictionaryList());

                if (currentPage++ % 10 == 0)
                {
                    _logger.LogInformation("Data extension download progress. CustomerKey={CustomerKey}, TotalRecords={TotalRecords}", dataExtensionCustomerKey, totalRecords);
                }
                else
                { 
                    _logger.LogDebug("Downloaded page {PageNumber}. CustomerKey={CustomerKey}, PageRecords={PageRecords}, TotalRecords={TotalRecords}", currentPage - 1, dataExtensionCustomerKey, currentPageSize, totalRecords);
                }
                if (results == null || results.Results == null || results.Results.links == null || results.Results.links == null || string.IsNullOrEmpty(results.Results.links.next))
                {
                    _logger.LogInformation("Completed data extension download. CustomerKey={CustomerKey}, TotalRecords={TotalRecords}, FileName={FileName}", dataExtensionCustomerKey, totalRecords, fileName);
                    break;
                }

                _logger.LogTrace($"Running Loop Again");
                url = $"{baseUrl}{results.Results.links.next}";
            } while (true);
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Failed to download data extension. CustomerKey={CustomerKey}, FileName={FileName}", dataExtensionCustomerKey, fileName);
            throw;
        }
        return totalRecords;
    }


    // TODO: Figure out how to Generic this and move down to BaseRestApi
    private async Task<RestResults<DataExtensionDataDto, string>> LoadApiCallAsync(
        string url
    )
    {
        _logger.LogTrace($"Attempting to {verb} to {url}");

        await SetAuthHeaderAsync();
        return await _restClientAsync.ExecuteRestMethodAsync<DataExtensionDataDto, string>(
            uri: new Uri(url),
            verb: verb,
            headers: _headers
            );
    }
}
