using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.Authentication;
using bleak.Martech.SalesforceMarketingCloud.Models.SfmcDtos;
using bleak.Martech.SalesforceMarketingCloud.Models;
using bleak.Martech.SalesforceMarketingCloud.Configuration;
using bleak.Martech.SalesforceMarketingCloud.Rest;
using bleak.Martech.SalesforceMarketingCloud.Fileops;
using System.Formats.Asn1;
using bleak.Martech.SalesforceMarketingCloud.Models.Helpers;

namespace bleak.Martech.SalesforceMarketingCloud.Sfmc.Rest.DataExtensions
{

    public class DataExtensionRestApi : BaseRestApi, IDataExtensionRestApi
    {
        public DataExtensionRestApi(
            IAuthRepository authRepository)
            : this(authRepository, new SfmcConnectionConfiguration())
        {
        }
        public DataExtensionRestApi(
            IAuthRepository authRepository,
            SfmcConnectionConfiguration config)
            : base(authRepository, config)
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
                string baseUrl = $"https://{_authRepository.Subdomain}.rest.marketingcloudapis.com/data";
                string url = $"{baseUrl}/v1/customobjectdata/key/{dataExtensionCustomerKey}/rowset?$page=1&$pageSize=2500";
                do
                {
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


                    if (_sfmcConnectionConfiguration.Debug) Console.WriteLine($"Current Page had {currentPageSize} records. There are now {totalRecords} Records Identified.");

                    if (results == null || results?.Results == null || results.Results.links == null || results!.Results.links == null)
                    {
                        if (_sfmcConnectionConfiguration.Debug) Console.WriteLine($"No more pages to process. Exiting loop.");
                        break;
                    }
                    if (results!.Results.links.next != null)
                    {
                        if (_sfmcConnectionConfiguration.Debug) Console.WriteLine($"Running Loop Again");
                        url = $"{baseUrl}{results.Results.links.next}";
                    }
                } while (true);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
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

        // TODO: Figure out how to Generic this and move down to BaseRestApi
        private RestResults<T, string> LoadApiWithRetry<T>(
            Func<string, RestResults<T, string>> loadApiCall,
            string url,
            string authenticationError,
            Action resolveAuthentication
            )
        {
            var results = loadApiCall(url);

            // Check if an error occurred and it matches the specified errorText
            if (results != null && results.UnhandledError != null && results.UnhandledError.Contains(authenticationError))
            {
                Console.WriteLine($"Unauthenticated: {results.UnhandledError}");

                // Resolve authentication
                resolveAuthentication();
                Console.WriteLine("Authentication Header has been reset");

                // Retry the REST method
                results = loadApiCall(url);

                Console.WriteLine("Press Enter to Continue");
                Console.ReadLine();
            }

            return results!;
        }


    }
}


