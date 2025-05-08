using bleak.Api.Rest;

namespace bleak.Martech.SalesforceMarketingCloud.Authentication
{
    public abstract class AuthRepositoryBase : IAuthRepository
    {
        public string Subdomain { get; protected set; }
        protected string ClientId { get; set; }
        protected string ClientSecret { get; set; }
        protected string MemberId { get; set; }
        protected const double Threshold = 600.07;
        private static Lazy<SfmcAuthToken> _cachedToken;
        public SfmcAuthToken Token => _cachedToken.Value;
        protected readonly JsonSerializer _jsonSerializer;
        protected readonly RestManager _restManager;
        private static readonly object _lock = new();

        protected AuthRepositoryBase(string subdomain, string clientId, string clientSecret, string memberId)
        {
            _jsonSerializer = new JsonSerializer();
            _restManager = new RestManager(_jsonSerializer, _jsonSerializer);

            Subdomain = subdomain;
            ClientId = clientId;
            ClientSecret = clientSecret;
            MemberId = memberId;
            _cachedToken = new Lazy<SfmcAuthToken>(LoadToken, true); // Thread-safe lazy loading
        }

        protected virtual bool IsTokenValid()
        {
            throw new NotImplementedException();
        }
        protected virtual SfmcAuthToken LoadToken()
        {   
            throw new NotImplementedException();
        }
        protected virtual void SaveToken(SfmcAuthToken token)
        {
            throw new NotImplementedException();
        }

        protected SfmcAuthToken Authenticate()
        {
            Console.WriteLine("Authenticating...");
            string tokenUri = $"https://{Subdomain}.auth.marketingcloudapis.com/v2/token";

            var authResults = _restManager.ExecuteRestMethod<SfmcAuthToken, string>(
                uri: new Uri(tokenUri),
                verb: HttpVerbs.POST,
                payload: new
                {
                    grant_type = "client_credentials",
                    client_id = ClientId,
                    client_secret = ClientSecret,
                    account_id = MemberId,
                },
                headers: new List<Header> { new Header { Name = "Content-Type", Value = "application/json" } }
            );

            if (authResults.Error != null)
            {
                throw new Exception($"Authentication failed: {authResults.Error}");
            }

            return authResults.Results;
        }

        public virtual  void ResolveAuthentication()
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
    }
}