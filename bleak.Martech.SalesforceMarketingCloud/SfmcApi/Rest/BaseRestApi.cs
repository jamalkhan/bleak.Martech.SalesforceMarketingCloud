using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.Authentication;
using bleak.Martech.SalesforceMarketingCloud.Configuration;
using Microsoft.Extensions.Logging;

namespace bleak.Martech.SalesforceMarketingCloud.Rest
{
    public class BaseRestApi<T>
    {
        protected readonly ILogger<T> _logger;

        protected readonly IRestManagerAsync _restManagerAsync;
        protected readonly IRestManager _restManager;
        protected readonly IAuthRepository _authRepository;
        protected readonly SfmcConnectionConfiguration _sfmcConnectionConfiguration;



        protected List<Header> _headers = new List<Header>
        {
            new Header() { Name = "Content-Type", Value = "application/json" },
        };


        public BaseRestApi(
            IRestManager restManager,
            IRestManagerAsync restManagerAsync,
            IAuthRepository authRepository,
            SfmcConnectionConfiguration config,
            ILogger<T> logger
            )
        {
            _logger = logger;
            _restManager = restManager;
            _restManagerAsync = restManagerAsync;
            _authRepository = authRepository;
            _sfmcConnectionConfiguration = config;
        }

        protected void SetAuthHeader()
        {
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
                new Header() { Name = "Authorization", Value = $"Bearer {_authRepository.Token.access_token}" }
            );
        }
        
        protected RestResults<T2, string> LoadApiWithRetry<T2>(
            Func<string, RestResults<T2, string>> loadApiCall,
            string url,
            string authenticationError,
            Action resolveAuthentication
            )
        {
            var results = loadApiCall(url);

            // Check if an error occurred and it matches the specified errorText
            if (results != null && results.UnhandledError != null && results.UnhandledError.Contains(authenticationError))
            {
                Console.WriteLine($"Unauthenticated: {results.UnhandledError}");

                // Resolve authentication
                resolveAuthentication();
                Console.WriteLine("Authentication Header has been reset");

                // Retry the REST method
                results = loadApiCall(url);

                Console.WriteLine("Press Enter to Continue");
                Console.ReadLine();
            }

            return results!;
        }
    }
}