using bleak.Api.Rest;
using System.Diagnostics;
using System.ServiceModel;
using System.Threading;

namespace bleak.Martech.SalesforceMarketingCloud.Authentication
{
    public partial class AuthRepository
    {
        private readonly string _subdomain;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _memberId;

        public string Subdomain => _subdomain;

        private readonly JsonSerializer _jsonSerializer;
        private readonly RestManager _restManager;
        private static readonly object _lock = new();
        private static Lazy<SfmcAuthToken> _cachedToken;
        private static readonly string AuthFilePath = Path.Combine(AppContext.BaseDirectory, "authentication.json");
        private const double Threshold = 600.07;

        public AuthRepository(RestManager restManager, JsonSerializer jsonSerializer, string subdomain, string clientId, string clientSecret, string memberId)
        {
            _subdomain = subdomain;
            _clientId = clientId;
            _clientSecret = clientSecret;
            _memberId = memberId;
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
            string tokenUri = $"https://{_subdomain}.auth.marketingcloudapis.com/v2/token";

            var authResults = _restManager.ExecuteRestMethod<SfmcAuthToken, string>(
                uri: new Uri(tokenUri),
                verb: HttpVerbs.POST,
                payload: new
                {
                    grant_type = "client_credentials",
                    client_id = _clientId,
                    client_secret = _clientSecret,
                    account_id = _memberId,
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