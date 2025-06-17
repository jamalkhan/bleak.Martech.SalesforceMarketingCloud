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
        protected readonly RestManager _restManager;
        
        public string Subdomain {get; private set; }
        public string ClientId {get; private set; }
        public string ClientSecret {get; private set; }
        public string MemberId {get; private set; }

        private static readonly object _lock = new object();
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

        public void ResolveAuthentication()
        {
            throw new NotImplementedException();
        }

        public MauiAuthRepository(SfmcConnection connection, ILogger<MauiAuthRepository> logger)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));
            Subdomain = connection.Subdomain;
            ClientId = connection.ClientId;
            ClientSecret = connection.ClientSecret;
            MemberId = connection.MemberId;
            _jsonSerializer = new JsonSerializer();
            _restManager = new RestManager(_jsonSerializer, _jsonSerializer);
        }
        public MauiAuthRepository(string subdomain, string clientId, string clientSecret, string memberId)
        {
            Subdomain = subdomain;
            ClientId = clientId;
            ClientSecret = clientSecret;
            MemberId = memberId;
            _jsonSerializer = new JsonSerializer();
            _restManager = new RestManager(_jsonSerializer, _jsonSerializer);
        }
    }
}