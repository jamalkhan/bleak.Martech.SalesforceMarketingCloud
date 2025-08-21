using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.Authentication;
using Microsoft.Extensions.Logging;
using SfmcApp.Models;

namespace bleak.Martech.SalesforceMarketingCloud.Sfmc.Models
{
    public partial class MauiAuthRepository : IAuthRepository
    {
        private const double TokenRetentionThreshold = 600.07; // seconds
        private DateTime _lastWriteTime = DateTime.MinValue;

        private readonly JsonSerializer _jsonSerializer;
        private readonly IRestClientAsync _restClientAsync;
        private readonly ILogger<MauiAuthRepository> _logger;

        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public string Subdomain { get; }
        public string ClientId { get; }
        public string ClientSecret { get; }
        public string MemberId { get; }

        /// <summary>
        /// DO NOT USE TOKEN PROPERTY DIRECTLY. Use GetTokenAsync() method to ensure valid token.
        /// </summary>
        public SfmcAuthToken Token => throw new NotImplementedException();

        private SfmcAuthToken? _token;

        public MauiAuthRepository(
            string subdomain,
            string clientId,
            string clientSecret,
            string memberId,
            JsonSerializer jsonSerializer,
            IRestClientAsync restClientAsync,
            ILogger<MauiAuthRepository> logger)
        {
            Subdomain = subdomain;
            ClientId = clientId;
            ClientSecret = clientSecret;
            MemberId = memberId;
            _jsonSerializer = jsonSerializer;
            _restClientAsync = restClientAsync;
            _logger = logger;
        }

        /// <summary>
        /// Always use this method to get a valid token. Will authenticate if expired/invalid.
        /// </summary>
        public async Task<SfmcAuthToken> GetTokenAsync()
        {
            if (!IsTokenValid())
            {
                await _semaphore.WaitAsync();
                try
                {
                    if (!IsTokenValid())
                    {
                        _token = await AuthenticateAsync().ConfigureAwait(false);
                        _lastWriteTime = DateTime.Now;
                    }
                }
                finally
                {
                    _semaphore.Release();
                }
            }

            return _token ?? throw new InvalidOperationException("Authentication failed.");
        }

        private bool IsTokenValid()
        {
            if (_token != null)
            {
                TimeSpan timeDifference = DateTime.Now - _lastWriteTime;
                return timeDifference.TotalSeconds <= TokenRetentionThreshold;
            }
            return false;
        }

        private async Task<SfmcAuthToken> AuthenticateAsync()
        {
            _logger.LogInformation("Starting authentication process");

            string tokenUri = $"https://{Subdomain}.auth.marketingcloudapis.com/v2/token";
            _logger.LogInformation($"Authentication URL: {tokenUri}");
            _logger.LogInformation($"Client ID: {ClientId}");
            _logger.LogInformation($"Member ID: {MemberId}");

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

                _logger.LogInformation("About to execute REST authentication call");

                var payload = new
                {
                    grant_type = "client_credentials",
                    client_id = ClientId,
                    client_secret = ClientSecret,
                    account_id = MemberId,
                };

                _logger.LogInformation("Authentication payload created, about to make REST call");

                var authResults = await _restClientAsync.ExecuteRestMethodAsync<SfmcAuthToken, string>(
                    uri: new Uri(tokenUri),
                    verb: HttpVerbs.POST,
                    payload: payload,
                    headers: new[]
                    {
                        new Header { Name = "Content-Type", Value = "application/json" }
                    },
                    cancellationToken: cts.Token
                ).ConfigureAwait(false);

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

        /// <summary>
        /// Force token refresh regardless of current state.
        /// </summary>
        public async Task ResolveAuthenticationAsync()
        {
            _logger.LogInformation("Resolving authentication - clearing current token");
            _token = null;
            _lastWriteTime = DateTime.MinValue;

            await _semaphore.WaitAsync();
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                _token = await AuthenticateAsync().ConfigureAwait(false);
                _lastWriteTime = DateTime.Now;
                _logger.LogInformation("Authentication resolved successfully");
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
