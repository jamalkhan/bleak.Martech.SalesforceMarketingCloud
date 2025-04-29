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
        private static readonly string AuthFilePath = Path.Combine(AppContext.BaseDirectory, "authentication.json");
        private const double Threshold = 600.07;

        public AuthRepository(RestManager restManager, JsonSerializer jsonSerializer)
        {
            _restManager = restManager;
            _jsonSerializer = jsonSerializer;
            _cachedToken = new Lazy<SfmcAuthToken>(LoadToken, true); // Thread-safe lazy loading
        }

        public SfmcAuthToken Token => _cachedToken.Value;

        public void ResolveAuthentication()
        {
            if (IsTokenValid())
            {
                Console.WriteLine("Using cached authentication file.");
                return;
            }

            lock (_lock) // Ensure only one thread reauthenticates
            {
                if (!IsTokenValid()) // Double-check after acquiring the lock
                {
                    Console.WriteLine("Authentication expired or not found. Re-authenticating...");
                    var newToken = Authenticate();
                    SaveToken(newToken);
                    _cachedToken = new Lazy<SfmcAuthToken>(() => newToken, true);
                }
            }
        }

        private bool IsTokenValid()
        {
            if (File.Exists(AuthFilePath))
            {
                DateTime lastWriteTime = File.GetLastWriteTime(AuthFilePath);
                TimeSpan timeDifference = DateTime.Now - lastWriteTime;
                if (timeDifference.TotalSeconds <= Threshold)
                {
                    return true;
                }
                File.Delete(AuthFilePath);
            }
            return false;
        }

        private SfmcAuthToken LoadToken()
        {
            if (File.Exists(AuthFilePath))
            {
                return _jsonSerializer.Deserialize<SfmcAuthToken>(File.ReadAllText(AuthFilePath));
            }
            throw new InvalidOperationException("No valid authentication file found.");
        }

        private void SaveToken(SfmcAuthToken token)
        {
            string json = _jsonSerializer.Serialize(token);
            File.WriteAllText(AuthFilePath, json);
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