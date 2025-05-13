using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.Authentication;
using bleak.Martech.SalesforceMarketingCloud.Configuration;

namespace bleak.Martech.SalesforceMarketingCloud.Rest
{
    public class BaseRestApi
    {
        protected readonly RestManager _restManager;
        protected readonly IAuthRepository _authRepository;
        protected readonly SfmcConnectionConfiguration _sfmcConnectionConfiguration;


        
        protected List<Header> _headers = new List<Header>
        {
            new Header() { Name = "Content-Type", Value = "application/json" },
        };


        public BaseRestApi(
            RestManager restManager, 
            IAuthRepository authRepository, 
            SfmcConnectionConfiguration sfmcConnectionConfiguration)
        {
            _restManager = restManager;
            _authRepository = authRepository;
            _sfmcConnectionConfiguration = sfmcConnectionConfiguration;
        }

        protected void SetAuthHeader()
        {
            if (_headers == null)
            {
                _headers = new List<Header>();
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
    }
}