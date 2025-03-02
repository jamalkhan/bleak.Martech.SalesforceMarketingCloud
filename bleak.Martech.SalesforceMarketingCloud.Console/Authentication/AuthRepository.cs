using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Configuration;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Authentication;
using bleak.Martech.SalesforceMarketingCloud.ContentBuilder;
using bleak.Martech.SalesforceMarketingCloud.ContentBuilder.SfmcPocos;
using System.Diagnostics;
using System.ServiceModel;
using System.Threading;

namespace bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Authentication
{
    public partial class AuthRepository
    {
        private readonly JsonSerializer _jsonSerializer;
        private readonly RestManager _restManager;
        private static readonly object _lock = new();
        private static Lazy<SfmcAuthToken> _cachedToken;
        public AuthRepository(RestManager restManager, JsonSerializer jsonSerializer)
        {
            _restManager = restManager;
            _jsonSerializer = jsonSerializer;
                    _cachedToken = new Lazy<SfmcAuthToken>(LoadToken, true); // Thread-safe lazy loading

        }

        public SfmcAuthToken Token => _cachedToken.Value;



        public void ResolveAuthentication()
        {
            string authFile = Path.Combine(AppContext.BaseDirectory, "authentication.json");
            double threshold = 600.07;

            if (File.Exists(authFile))
            {
                DateTime lastWriteTime = File.GetLastWriteTime(authFile);
                TimeSpan timeDifference = DateTime.Now - lastWriteTime;

                if (timeDifference.TotalSeconds <= threshold)
                {
                    Console.WriteLine($"Using cached authentication file. TimeDifference: {timeDifference.TotalSeconds}s");
                    _cachedToken = new Lazy<SfmcAuthToken>(() => LoadToken(), true);
                    return;
                }

                Console.WriteLine("Authentication expired. Re-authenticating...");
                File.Delete(authFile);
            }
            else
            {
                Console.WriteLine("No authentication file found. Authenticating...");
            }

            lock (_lock) // Ensure only one thread reauthenticates
            {
                if (!File.Exists(authFile)) // Double-check after acquiring the lock
                {
                    var newToken = Authenticate();
                    SaveToken(newToken, authFile);
                    _cachedToken = new Lazy<SfmcAuthToken>(() => newToken, true);
                }
            }
        }

        private SfmcAuthToken LoadToken()
        {
            string authFile = Path.Combine(AppContext.BaseDirectory, "authentication.json");

            if (File.Exists(authFile))
            {
                return _jsonSerializer.Deserialize<SfmcAuthToken>(File.ReadAllText(authFile));
            }
            throw new InvalidOperationException("No valid authentication file found.");
        }

        private void SaveToken(SfmcAuthToken token, string authFile)
        {
            string json = _jsonSerializer.Serialize(token);
            File.WriteAllText(authFile, json);
        }
        
         private SfmcAuthToken Authenticate()
        {
            Console.WriteLine("Authenticating...");
            string tokenUri = $"https://{AppConfiguration.Instance.Subdomain}.auth.marketingcloudapis.com/v2/token";

            var authResults = _restManager.ExecuteRestMethod<SfmcAuthToken, string>(
                uri: new Uri(tokenUri),
                verb: HttpVerbs.POST,
                payload: new
                {
                    grant_type = "client_credentials",
                    client_id = AppConfiguration.Instance.ClientId,
                    client_secret = AppConfiguration.Instance.ClientSecret,
                    account_id = AppConfiguration.Instance.MemberId
                },
                headers: new List<Header> { new Header { Name = "Content-Type", Value = "application/json" } }
            );

            if (authResults.Error != null)
            {
                throw new Exception($"Authentication failed: {authResults.Error}");
            }

            return authResults.Results;
        }
    }
}