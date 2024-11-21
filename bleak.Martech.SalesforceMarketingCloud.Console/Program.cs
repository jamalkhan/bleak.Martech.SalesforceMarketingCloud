using bleak.Api.Rest;
using bleak.Api.Rest.Common;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Configuration;
using bleak.Martech.SalesforceMarketingCloud.Wsdl;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Authentication;
using bleak.Martech.SalesforceMarketingCloud.ContentBuilder;

namespace bleak.Martech.SalesforceMarketingCloud.ConsoleApp
{
    public static class Program
    {
        static JsonSerializer serializer = new JsonSerializer();
        static CoreRestManager rm = new CoreRestManager(serializer, serializer);
    
        private static void Main(string[] args)
        {
            // See https://aka.ms/new-console-template for more information
            Console.WriteLine("Hello, World!");
            var authToken = ResolveAuthentication();

            const int rootFolder = 0;
            GetFolderTree(token: authToken, parent_id:rootFolder);

            // Write JSON content to the file


        }

        private static SfmcAuthToken ResolveAuthentication()
        {
            string authFile = Path.Combine(AppContext.BaseDirectory, "authentication.json");
            const int thresholdInSeconds = 600;
            SfmcAuthToken authToken;
            if (File.Exists(authFile))
            {
                // Get the last write time of the file
                DateTime lastWriteTime = File.GetLastWriteTime(authFile);

                // Calculate the time difference
                TimeSpan timeDifference = DateTime.Now - lastWriteTime;

                // Check if the file is older than 600 seconds
                if (timeDifference.TotalSeconds > thresholdInSeconds)
                {
                    Console.WriteLine("The file is older than 600 seconds.");
                    File.Delete(authFile);
                    authToken = Authenticate();
                    string json = serializer.Serialize(authToken);
                    File.WriteAllText(authFile, json);
                }
                else
                {
                    Console.WriteLine("The file is not older than 600 seconds.");
                    authToken = serializer.Deserialize<SfmcAuthToken>(File.ReadAllText(authFile));
                }
            }
            else
            {
                authToken = Authenticate();
                string json = serializer.Serialize(authToken);
                File.WriteAllText(authFile, json);
            }

            return authToken;
        }

        private static SfmcAuthToken Authenticate()
        {
            RequestResponseSummary<SfmcAuthToken, string> authResults;

            string tokenUri = "https://" + AppConfiguration.Instance.Subdomain + ".auth.marketingcloudapis.com/v2/token";
            Console.WriteLine($"Trying to authenticate to {tokenUri}");

            authResults = rm.ExecuteRestMethod<SfmcAuthToken, string>(
                uri: new Uri(tokenUri),
                verb: Api.Rest.Common.HttpVerbs.POST,
                payload: new
                {
                    grant_type = "client_credentials",
                    client_id = AppConfiguration.Instance.ClientId,
                    client_secret = AppConfiguration.Instance.ClientSecret,
                    account_id = AppConfiguration.Instance.MemberId
                },
                headers: new List<Header>() { new Header() { Name = "Content-Type", Value = "application/json" } }
                );

            //File.WriteAllBytes("temp.token", authResults.Results.)
            Console.WriteLine($"authResults.Value = {authResults.Results}");
            Console.WriteLine($"authResults.Error = {authResults.Error}");

            return authResults.Results;
        }

        private static string GetFolderTree(SfmcAuthToken token, int parent_id = 0, int page = 1)
        {
            

            const int pageSize = 100;

            string uri = $"https://{AppConfiguration.Instance.Subdomain}.rest.marketingcloudapis.com/asset/v1/content/categories?page={page}&pagesize={pageSize}$orderBy=name&$filter=parentId eq {parent_id}";
            Console.WriteLine($"Trying to download to {uri}");

            RequestResponseSummary<SfmcRestWrapper<SfmcFolder>>, string> results;
            results = rm.ExecuteRestMethod<string, string>(
                uri: new Uri(uri),
                verb: Api.Rest.Common.HttpVerbs.GET,
                headers:
                    new List<Header>()
                    { 
                        new Header() { Name = "Content-Type", Value = "application/json" } ,
                        new Header() { Name = "Authorization", Value = $"Bearer {token.access_token}" }
                    }
                );

            //File.WriteAllBytes("temp.token", authResults.Results.)
            Console.WriteLine($"authResults.Value = {results.Results}");
            Console.WriteLine($"authResults.Error = {results.Error}");

            return results.Results;
        }
    }
}