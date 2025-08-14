using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.Authentication;
using Microsoft.Extensions.Logging;
using SfmcApp.Models;

namespace bleak.Martech.SalesforceMarketingCloud.Sfmc.Models
{
    public partial class MauiAuthRepository : IAuthRepository
    {
        const double TokenRetionThreshold = 600.07;
        private DateTime _lastWriteTime = DateTime.Now;

        protected readonly JsonSerializer _jsonSerializer;
        protected readonly IRestClientAsync _restClientAsync;
        protected readonly ILogger<MauiAuthRepository> _logger;
        
        public string Subdomain { get; private set; }
        public string ClientId {get; private set; }
        public string ClientSecret {get; private set; }
        public string MemberId {get; private set; }

        private static readonly Lock _lock = new();
        public SfmcAuthToken? _token;
        public SfmcAuthToken Token
        {
            get
            {
                if (!IsTokenValid())
                {
                    lock (_lock)
                    {
                        if (!IsTokenValid())
                        {
                            _token = Authenticate();
                            _lastWriteTime = DateTime.Now;
                        }
                    }
                }
                
                return _token ?? throw new InvalidOperationException("Authentication failed.");
            }
        }

        bool IsTokenValid()
        {
            if (_token != null)
            {
                TimeSpan timeDifference = DateTime.Now - _lastWriteTime;
                return timeDifference.TotalSeconds <= TokenRetionThreshold;
            }
            return false;
        }

        protected SfmcAuthToken Authenticate()
        {
            try
            {
                return AuthenticateAsync().GetAwaiter().GetResult();
            }
            catch (AggregateException ae)
            {
                throw ae.InnerException ?? ae;
            }
        }


        public async Task<bool> TestNetworkConnectivityAsync()
        {
            try
            {
                _logger.LogInformation("Testing network connectivity...");
                
                // Add overall timeout to prevent hanging
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
                
                // Try a simpler approach first - just test if we can resolve a DNS name
                _logger.LogInformation("Testing DNS resolution...");
                var hostEntry = await System.Net.Dns.GetHostEntryAsync("www.google.com").WaitAsync(cts.Token);
                _logger.LogInformation($"DNS resolution successful: {hostEntry.HostName}");
                
                _logger.LogInformation("Creating HttpClient...");
                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(10);
                _logger.LogInformation("HttpClient created with 10 second timeout");
                
                _logger.LogInformation("About to make HTTP request to Google...");
                var response = await httpClient.GetAsync("https://www.google.com", cts.Token);
                _logger.LogInformation($"Network connectivity test successful: {response.StatusCode}");
                return response.IsSuccessStatusCode;
            }
            catch (OperationCanceledException)
            {
                _logger.LogError("Network connectivity test timed out after 15 seconds");
                return false;
            }
            /*catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Network connectivity test timed out");
                return false;
            }
            */
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network connectivity test failed with HttpRequestException");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Network connectivity test failed with unexpected exception");
                return false;
            }
        }

        protected async Task<SfmcAuthToken> AuthenticateAsync()
        {
            _logger.LogInformation("Starting authentication process");
            Console.WriteLine("Authenticating...");
            
            // Test network connectivity first (with timeout)
            try
            {
                var networkTest = await TestNetworkConnectivityAsync();
                if (!networkTest)
                {
                    _logger.LogWarning("Network connectivity test failed - proceeding with authentication anyway");
                    // Don't throw here, just log a warning and continue
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Network connectivity test failed with exception - proceeding with authentication anyway");
                // Don't throw here, just log a warning and continue
            }
            
            string tokenUri = $"https://{Subdomain}.auth.marketingcloudapis.com/v2/token";
            _logger.LogInformation($"Authentication URL: {tokenUri}");
            _logger.LogInformation($"Client ID: {ClientId}");
            _logger.LogInformation($"Member ID: {MemberId}");

            try
            {
                // Add timeout to prevent hanging
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                
                _logger.LogInformation("About to execute REST authentication call");
                
                // Create the payload for logging
                var payload = new
                {
                    grant_type = "client_credentials",
                    client_id = ClientId,
                    client_secret = ClientSecret,
                    account_id = MemberId,
                };
                
                _logger.LogInformation($"Authentication payload created, about to make REST call");
                
                var authResults = await _restClientAsync.ExecuteRestMethodAsync<SfmcAuthToken, string>(
                    uri: new Uri(tokenUri),
                    verb: HttpVerbs.POST,
                    payload: payload,
                    headers:
                    [
                        new Header { Name = "Content-Type", Value = "application/json" }
                    ]
                ).WaitAsync(cts.Token);
                
                _logger.LogInformation("REST authentication call completed");

                if (authResults.Error != null)
                {
                    _logger.LogError($"Authentication failed: {authResults.Error}");
                    throw new Exception($"Authentication failed: {authResults.Error}");
                }

                _logger.LogInformation("Authentication completed successfully");
                return authResults.Results;
            }
            catch (OperationCanceledException)
            {
                _logger.LogError("Authentication timed out after 30 seconds");
                throw new TimeoutException("Authentication timed out. Please check your connection and try again.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Authentication process failed");
                throw;
            }
        }


        public void ResolveAuthentication()
        {
            throw new NotImplementedException();
        }

        public async Task ResolveAuthenticationAsync()
        {
            _logger.LogInformation("Resolving authentication - clearing current token");
            _token = null;
            _lastWriteTime = DateTime.MinValue;
            
            try
            {
                // Add timeout to prevent hanging
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                
                // Force a new authentication
                _token = await AuthenticateAsync().WaitAsync(cts.Token);
                _lastWriteTime = DateTime.Now;
                _logger.LogInformation("Authentication resolved successfully");
            }
            catch (OperationCanceledException)
            {
                _logger.LogError("Authentication resolution timed out after 30 seconds");
                throw new TimeoutException("Authentication resolution timed out. Please check your connection and try again.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Authentication resolution failed");
                throw;
            }
        }

        public MauiAuthRepository
        (
            string subdomain,
            string clientId,
            string clientSecret,
            string memberId,
            JsonSerializer jsonSerializer,
            IRestClientAsync restClientAsync,
            ILogger<MauiAuthRepository> logger
        )
        {
            Subdomain = subdomain;
            ClientId = clientId;
            ClientSecret = clientSecret;
            MemberId = memberId;
            _jsonSerializer = jsonSerializer;
            _restClientAsync = restClientAsync;
            _logger = logger;
        }
    }
}