using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.Authentication;
using bleak.Martech.SalesforceMarketingCloud.Models.SfmcDtos;
using bleak.Martech.SalesforceMarketingCloud.Configuration;
using bleak.Martech.SalesforceMarketingCloud.Rest;
using bleak.Martech.SalesforceMarketingCloud.Fileops;
using bleak.Martech.SalesforceMarketingCloud.Models.Helpers;
using Microsoft.Extensions.Logging;

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
        IAuthRepository authRepository,
        SfmcConnectionConfiguration config,
        ILogger<DataExtensionRestApi> logger
    )
        : base
        (
            restManager: new RestManager(new JsonSerializer(), new JsonSerializer()),
            restManagerAsync: new RestManager(new JsonSerializer(), new JsonSerializer()),
            authRepository: authRepository,
            config: config,
            logger: logger
        )
    {
    }

    private HttpVerbs verb = HttpVerbs.GET;
    //"https://{{et_subdomain}}.rest.marketingcloudapis.com/data/v1/customobjectdata/key/Person_NA/rowset";


    public async Task<long> DownloadDataExtensionAsync(
        string dataExtensionCustomerKey,
        IFileWriter fileWriter,
        string fileName
    )
    {
        return await Task.FromResult(DownloadDataExtension(dataExtensionCustomerKey, fileWriter, fileName));
    }

    public long DownloadDataExtension
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
                RestResults<DataExtensionDataDto, string> results = LoadApiWithRetry<DataExtensionDataDto>(
                    loadApiCall: LoadApiCall,
                    url: url,
                    authenticationError: "401",
                    resolveAuthentication: _authRepository.ResolveAuthentication
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

                _logger.LogInformation($"Running Loop Again");
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
    private RestResults<DataExtensionDataDto, string> LoadApiCall(
        string url
    )
    {
        if (_sfmcConnectionConfiguration.Debug) { Console.WriteLine($"Attempting to {verb} to {url} with accessToken: {_authRepository.Token.access_token}"); }

        SetAuthHeader();
        var results = _restManager.ExecuteRestMethod<DataExtensionDataDto, string>(
            uri: new Uri(url),
            verb: verb,
            headers: _headers
            );

        return results!;
    }
}
