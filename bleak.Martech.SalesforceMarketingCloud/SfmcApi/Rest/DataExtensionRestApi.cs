using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.Authentication;
using bleak.Martech.SalesforceMarketingCloud.Models.SfmcDtos;
using bleak.Martech.SalesforceMarketingCloud.Models;
using bleak.Martech.SalesforceMarketingCloud.Configuration;
using bleak.Martech.SalesforceMarketingCloud.Rest;

namespace bleak.Martech.SalesforceMarketingCloud.Rest
{

    public class DataExtensionRestApi : BaseRestApi
    {
        public DataExtensionRestApi(
            RestManager restManager, 
            IAuthRepository authRepository)
            : this(restManager, authRepository, new SfmcConnectionConfiguration())
        {
        }
        public DataExtensionRestApi(
            RestManager restManager, 
            IAuthRepository authRepository, 
            SfmcConnectionConfiguration sfmcConnectionConfiguration)
            : base(restManager, authRepository, sfmcConnectionConfiguration) 
        {
        }

        private HttpVerbs verb = HttpVerbs.GET;
        //"https://{{et_subdomain}}.rest.marketingcloudapis.com/data/v1/customobjectdata/key/Person_NA/rowset";


        public DataExtensionDataDto DownloadDataExtension(
            string dataExtensionCustomerKey, 
            int page = 1)
        {
            int currentPageSize;
            try
            {
                RestResults<DataExtensionDataDto, string> results;
                
                string url = $"https://{_authRepository.Subdomain}.rest.marketingcloudapis.com/data/v1/customobjectdata/key/{dataExtensionCustomerKey}/rowset?$page={page}";
                
                results = LoadApiWithRetry<DataExtensionDataDto>(
                    loadApiCall: LoadApiCall,
                    url: url,
                    authenticationError: "401", 
                    resolveAuthentication: _authRepository.ResolveAuthentication
                );

                if (_sfmcConnectionConfiguration.Debug) Console.WriteLine($"results.Value = {results?.Results}");
                if (results?.Error != null) Console.WriteLine($"results.Error = {results.Error}");

                currentPageSize = results!.Results.items.Count();
                if (_sfmcConnectionConfiguration.Debug) Console.WriteLine($"Current Page had {currentPageSize} records. There are now {results!.Results.items.Count()} Total Folders Identified.");

                if (_sfmcConnectionConfiguration.PageSize == currentPageSize)
                {
                    if (_sfmcConnectionConfiguration.Debug) Console.WriteLine($"Running Loop Again");
                }
                return results!.Results;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                throw;
            }
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


