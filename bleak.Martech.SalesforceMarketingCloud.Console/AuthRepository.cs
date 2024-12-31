using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Configuration;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Authentication.SfmcPocos;
using bleak.Martech.SalesforceMarketingCloud.ContentBuilder;
using bleak.Martech.SalesforceMarketingCloud.ContentBuilder.SfmcPocos;
using System.Diagnostics;
using System.ServiceModel;

namespace bleak.Martech.SalesforceMarketingCloud.ConsoleApp
{
    public partial class AuthRepository
    {
        JsonSerializer _jsonSerializer;
        RestManager _restManager;
        public AuthRepository(RestManager restManager, JsonSerializer jsonSerializer)
        {
            _restManager = restManager;
            _jsonSerializer = jsonSerializer;
        }

        SfmcAuthToken token = new();
        public SfmcAuthToken Token
        {
            get
            {
                return token;
            }
        }


        public void ResolveAuthentication()
        {
            string authFile = Path.Combine(AppContext.BaseDirectory, "authentication.json");
            double threshold = 600.07;
            if (File.Exists(authFile))
            {
                // Get the last write time of the file
                DateTime lastWriteTime = File.GetLastWriteTime(authFile);

                // Calculate the time difference
                TimeSpan timeDifference = DateTime.Now - lastWriteTime;

                // Check if the file is older than 600 seconds
                if (timeDifference.TotalSeconds > threshold)
                {
                    Console.WriteLine("The file is older than 600 seconds.");
                    Console.WriteLine($"Deleting file {authFile}");
                    File.Delete(authFile);
                    Console.WriteLine($"Authenticating");
                    token = Authenticate();
                    if (AppConfiguration.Instance.Debug) Console.WriteLine($"Authenticated: {Token.access_token}");
                    string json = _jsonSerializer.Serialize(token);
                    if (AppConfiguration.Instance.Debug) Console.WriteLine($"Writing file {authFile}");
                    File.WriteAllText(authFile, json);
                    Thread.Sleep(1000);
                }
                else
                {
                    Console.WriteLine($"The file is not older than 600 seconds. timeDifference.TotalSeconds: {timeDifference.TotalSeconds}; threshold: {threshold}");
                    token = _jsonSerializer.Deserialize<SfmcAuthToken>(File.ReadAllText(authFile));
                }
            }
            else
            {
                Console.WriteLine("No file exists");
                Console.WriteLine($"Authenticating");
                token = Authenticate();
                if (AppConfiguration.Instance.Debug) Console.WriteLine($"Authenticated: {Token.access_token}");
                string json = _jsonSerializer.Serialize(token);
                Console.WriteLine($"Writing file {authFile}");
                File.WriteAllText(authFile, json);
                Thread.Sleep(1000);
            }
        }


        
        private SfmcAuthToken Authenticate()
        {
            Console.WriteLine($"Authenticating...........");
            RestResults<SfmcAuthToken, string> authResults;

            string tokenUri = "https://" + AppConfiguration.Instance.Subdomain + ".auth.marketingcloudapis.com/v2/token";
            Console.WriteLine($"Trying to authenticate to {tokenUri}");

            authResults = _restManager.ExecuteRestMethod<SfmcAuthToken, string>(
                uri: new Uri(tokenUri),
                verb: HttpVerbs.POST,
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
            if (AppConfiguration.Instance.Debug) Console.WriteLine($"authResults.Value = {authResults.Results}");
            if (authResults.Error != null) Console.WriteLine($"authResults.Error = {authResults.Error}");

            return authResults.Results;
        }
    }
}