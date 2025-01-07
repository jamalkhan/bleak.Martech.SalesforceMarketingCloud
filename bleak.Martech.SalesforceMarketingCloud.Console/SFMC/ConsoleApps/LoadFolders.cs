using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Configuration;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Authentication;
using bleak.Martech.SalesforceMarketingCloud.ContentBuilder;
using bleak.Martech.SalesforceMarketingCloud.ContentBuilder.SfmcPocos;
using System.Diagnostics;
using System.ServiceModel;
using System.Net.Http.Headers;

namespace bleak.Martech.SalesforceMarketingCloud.ConsoleApp.ConsoleApps
{
    public class LoadFolders
    {
        RestManager _restManager;
        AuthRepository _authRepository;
        public LoadFolders(RestManager restManager, AuthRepository authRepository)
        {
            _restManager = restManager;
            _authRepository = authRepository;
        }

        private HttpVerbs verb = HttpVerbs.GET;
        private List<Header> headers = new List<Header>
        {
            new Header() { Name = "Content-Type", Value = "application/json" },
        };

        public List<FolderObject> GetFolderTree()
        {
            int page = 1;
            int currentPageSize = 0;
            
            var sfmcFolders = new List<SfmcFolder>();
            do
            {
                currentPageSize = LoadFolder(page, sfmcFolders);
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

        private int LoadFolder(int page, List<SfmcFolder> sfmcFolders)
        {
            int currentPageSize;
            try
            {
                if (AppConfiguration.Instance.Debug) { Console.WriteLine($"Loading Folder Page #{page}"); }
                
                RestResults<SfmcRestWrapper<SfmcFolder>, string> results;
                string url = $"https://{AppConfiguration.Instance.Subdomain}.rest.marketingcloudapis.com/asset/v1/content/categories?$page={page}&$pagesize={AppConfiguration.Instance.PageSize}";
                
                results = ExecuteRestMethodWithRetry(
                    loadFolderApiCall: LoadFolderApiCall,
                    url: url,
                    authenticationError: "401", 
                    resolveAuthentication: _authRepository.ResolveAuthentication
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

        private RestResults<SfmcRestWrapper<SfmcFolder>, string> LoadFolderApiCall(
            string url
        )
        {
            // string opens = await GetTrackingData($"{baseUri}/messageDefinitionSends/key:{definitionKey}/tracking/opens", accessToken);
            // string clicks = await GetTrackingData($"{baseUri}/messageDefinitionSends/key:{definitionKey}/tracking/clicks", accessToken);
            // string bounces = await GetTrackingData($"{baseUri}/messageDefinitionSends/key:{definitionKey}/tracking/bounces", accessToken);
            // string sends = await GetTrackingData($"{baseUri}/messageDefinitionSends/key:{definitionKey}/tracking", accessToken);

            if (AppConfiguration.Instance.Debug) { Console.WriteLine($"Attempting to {verb} to {url} with accessToken: {_authRepository.Token.access_token}"); }

            var headersWithAuth = SetAuthHeader(headers);

            var results = _restManager.ExecuteRestMethod<SfmcRestWrapper<SfmcFolder>, string>(
                uri: new Uri(url),
                verb: verb,
                headers: headersWithAuth
                );

            return results!;
        }

        private RestResults<SfmcRestWrapper<SfmcFolder>, string> ExecuteRestMethodWithRetry(
            Func<string, RestResults<SfmcRestWrapper<SfmcFolder>, string>> loadFolderApiCall,
            string url,
            string authenticationError,
            Action resolveAuthentication
            )
        {
            var results = loadFolderApiCall(url);

            // Check if an error occurred and it matches the specified errorText
            if (results != null && results.UnhandledError != null && results.UnhandledError.Contains(authenticationError))
            {
                Console.WriteLine($"Unauthenticated: {results.UnhandledError}");

                // Resolve authentication
                resolveAuthentication();
                Console.WriteLine("Authentication Header has been reset");

                // Retry the REST method
                results = loadFolderApiCall(url);

                Console.WriteLine("Press Enter to Continue");
                Console.ReadLine();
            }

            return results!;
        }

        private List<Header> SetAuthHeader(List<Header> headers)
        {
            var headersWithAuth = new List<Header>();

            foreach (var header in headers)
            {
                headersWithAuth.Add(new Header() { Name = header.Name, Value = header.Value });
            }

            headersWithAuth.Add(
                new Header() { Name = "Authorization", Value = $"Bearer {_authRepository.Token.access_token}" }
            );

            return headersWithAuth;
        }
    }
}