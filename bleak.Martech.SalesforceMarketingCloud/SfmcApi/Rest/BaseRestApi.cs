using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.Authentication;
using bleak.Martech.SalesforceMarketingCloud.Configuration;
using Microsoft.Extensions.Logging;

namespace bleak.Martech.SalesforceMarketingCloud.Rest
{
    public class BaseRestApi<T>
    {
        protected readonly ILogger<T> _logger;
        protected readonly IRestClientAsync _restClientAsync;
        protected readonly IAuthRepository _authRepository;
        protected readonly SfmcConnectionConfiguration _sfmcConnectionConfiguration;

        protected List<Header> _headers = new List<Header>
        {
            new() { Name = "Content-Type", Value = "application/json" },
        };

        public BaseRestApi(
            IRestClientAsync restClientAsync,
            IAuthRepository authRepository,
            SfmcConnectionConfiguration config,
            ILogger<T> logger
            )
        {
            _logger = logger;
            _restClientAsync = restClientAsync;
            _authRepository = authRepository;
            _sfmcConnectionConfiguration = config;
        }

        protected async Task SetAuthHeaderAsync()
        {
            var token = await _authRepository.GetTokenAsync();
            _logger.LogInformation($"Access token available: {!string.IsNullOrEmpty(token?.access_token)}");

            if (_headers == null)
            {
                _headers = [];
            }

            // Remove existing Authorization header if it exists
            if (_headers.Any(h => h.Name == "Authorization"))
            {
                _headers.Remove(_headers.First(h => h.Name == "Authorization"));
            }

            _headers.Add(
                new Header() { Name = "Authorization", Value = $"Bearer {token?.access_token}" }
            );
            _logger.LogTrace("Auth header set, about to execute REST call");
        }
        
        protected async Task<RestResults<T2, string>> LoadApiWithRetryAsync<T2>(
            Func<string, Task<RestResults<T2, string>>> loadApiCallAsync,
            string url,
            string authenticationError,
            Func<Task> resolveAuthenticationAsync
            )
        {
            var results = await loadApiCallAsync(url);

            // Check if an error occurred and it matches the specified errorText
            if (results != null && results.UnhandledError != null && results.UnhandledError.Contains(authenticationError))
            {
            _logger.LogWarning($"Unauthenticated: {results.UnhandledError}");

            // Resolve authentication
            await resolveAuthenticationAsync();
            _logger.LogInformation("Authentication Header has been reset");

            // Retry the REST method
            results = await loadApiCallAsync(url);
            }

            return results!;
        }
    }
}