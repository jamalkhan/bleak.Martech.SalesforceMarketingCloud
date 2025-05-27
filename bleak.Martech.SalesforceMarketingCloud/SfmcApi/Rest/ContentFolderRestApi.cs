using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.Authentication;
using bleak.Martech.SalesforceMarketingCloud.Models.SfmcDtos;
using bleak.Martech.SalesforceMarketingCloud.Models;
using bleak.Martech.SalesforceMarketingCloud.Configuration;
using bleak.Martech.SalesforceMarketingCloud.Rest;

namespace bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Sfmc.Rest.Content
{
    public class ContentFolderRestApi : BaseRestApi
    {

        private HttpVerbs verb = HttpVerbs.GET;

        public ContentFolderRestApi(
            IAuthRepository authRepository)
            : this(authRepository, new SfmcConnectionConfiguration())
        {
        }
        public ContentFolderRestApi(
            IAuthRepository authRepository,
            SfmcConnectionConfiguration config)
            : base(authRepository: authRepository,config: config)
        {
        }
        

        public async Task<List<FolderObject>> GetFolderTreeAsync()
        {
            return await Task.Run(() => GetFolderTree());
        }
        
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
            while (_sfmcConnectionConfiguration.PageSize == currentPageSize);

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
                if (_sfmcConnectionConfiguration.Debug) { Console.WriteLine($"Loading Folder Page #{page}"); }
                
                RestResults<SfmcRestWrapper<SfmcFolder>, string> results;
                //asset/v1/content/categories
                string url = $"https://{_authRepository.Subdomain}.rest.marketingcloudapis.com/asset/v1/content/categories/?$page={page}&$pagesize={_sfmcConnectionConfiguration.PageSize}";
                
                results = ExecuteRestMethodWithRetry(
                    loadFolderApiCall: LoadFolderApiCall,
                    url: url,
                    authenticationError: "401", 
                    resolveAuthentication: _authRepository.ResolveAuthentication
                );

                if (_sfmcConnectionConfiguration.Debug) Console.WriteLine($"results.Value = {results?.Results}");
                if (results?.Error != null) Console.WriteLine($"results.Error = {results.Error}");

                currentPageSize = results!.Results.items.Count();
                sfmcFolders.AddRange(results.Results.items);
                if (_sfmcConnectionConfiguration.Debug) Console.WriteLine($"Current Page had {currentPageSize} records. There are now {sfmcFolders.Count()} Total Folders Identified.");

                if (_sfmcConnectionConfiguration.PageSize == currentPageSize)
                {
                    if (_sfmcConnectionConfiguration.Debug) Console.WriteLine($"Running Loop Again");
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
            if (_sfmcConnectionConfiguration.Debug) { Console.WriteLine($"Attempting to {verb} to {url} with accessToken: {_authRepository.Token.access_token}"); }

            SetAuthHeader();
            var results = _restManager.ExecuteRestMethod<SfmcRestWrapper<SfmcFolder>, string>(
                uri: new Uri(url),
                verb: verb,
                headers: _headers
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
    }
}