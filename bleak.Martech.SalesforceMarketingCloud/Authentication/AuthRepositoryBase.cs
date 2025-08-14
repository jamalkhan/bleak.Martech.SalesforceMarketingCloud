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
        private static Lazy<SfmcAuthToken>? _cachedToken;
        public SfmcAuthToken Token => _cachedToken != null ? _cachedToken.Value : throw new InvalidOperationException("_cachedToken is not initialized.");
        protected readonly IRestClientAsync _restClientAsync;
        private static readonly object _lock = new();

        protected AuthRepositoryBase(
            IRestClientAsync restClientAsync,
            string subdomain,
            string clientId,
            string clientSecret,
            string memberId)
        {
            _restClientAsync = restClientAsync;
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
        protected virtual Task SaveTokenAsync(SfmcAuthToken token)
        {
            throw new NotImplementedException();
        }

        protected SfmcAuthToken Authenticate()
        {
            try
            {
                return AuthenticateAsync().GetAwaiter().GetResult();
            }
            catch (AggregateException ae)
            {
                // Unwrap AggregateException for better error reporting
                throw ae.InnerException ?? ae;
            }
        }

        protected async Task<SfmcAuthToken> AuthenticateAsync()
        {
            Console.WriteLine("Authenticating...");
            string tokenUri = $"https://{Subdomain}.auth.marketingcloudapis.com/v2/token";

            var authResults = await _restClientAsync.ExecuteRestMethodAsync<SfmcAuthToken, string>(
                uri: new Uri(tokenUri),
                verb: HttpVerbs.POST,
                payload: new
                {
                    grant_type = "client_credentials",
                    client_id = ClientId,
                    client_secret = ClientSecret,
                    account_id = MemberId,
                },
                headers: new List<Header>
                {
                    new Header { Name = "Content-Type", Value = "application/json" }
                }
            );

            if (authResults.Error != null)
            {
                throw new Exception($"Authentication failed: {authResults.Error}");
            }

            return authResults.Results;
        }


        public virtual void ResolveAuthentication()
        {
            try
            {
                ResolveAuthenticationAsync().GetAwaiter().GetResult();
            }
            catch (AggregateException ae)
            {
                // Optional: unwrap for clearer error messages
                throw ae.InnerException ?? ae;
            }
        }
        
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public virtual async Task ResolveAuthenticationAsync()
        {
            if (IsTokenValid())
            {
                Console.WriteLine("Using cached authentication file.");
                return;
            }

            // Ensure only one thread reauthenticates at a time
            // Use SemaphoreSlim for async compatibility
            await _semaphore.WaitAsync();
            try
            {
                if (!IsTokenValid()) // Double-check after acquiring the lock
                {
                    Console.WriteLine("Authentication expired or not found. Re-authenticating...");
                    var newToken = await AuthenticateAsync();
                    await SaveTokenAsync(newToken);
                    _cachedToken = new Lazy<SfmcAuthToken>(() => newToken, true);
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}