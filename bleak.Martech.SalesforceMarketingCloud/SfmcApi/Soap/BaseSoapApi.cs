using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.Authentication;
using bleak.Martech.SalesforceMarketingCloud.Configuration;

namespace bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Sfmc.Soap
{
    public abstract partial class BaseSoapApi
    {
        protected string url { get; private set;}
        protected AuthRepository _authRepository { get; private set;}
        protected RestManager _restManager { get; private set;}
        protected SfmcConnectionConfiguration _sfmcConnectionConfiguration { get; private set;}

        public BaseSoapApi(AuthRepository authRepository, SfmcConnectionConfiguration sfmcConnectionConfiguration)
        {
            _sfmcConnectionConfiguration = sfmcConnectionConfiguration;
            _authRepository = authRepository;
            var soapSerializer = new SoapSerializer();
            _restManager = new RestManager(soapSerializer, soapSerializer);
            url = $"https://{_authRepository.Subdomain}.soap.marketingcloudapis.com/Service.asmx";
        }
        
        protected List<Header> BuildHeaders()
        {
            var headers = new List<Api.Rest.Header>();
            headers.Add(new Api.Rest.Header() { Name = "Content-Type", Value = "text/xml" });
            headers.Add(new Api.Rest.Header() { Name = "Accept", Value = "/" });
            headers.Add(new Api.Rest.Header() { Name = "Cache-Control", Value = "no-cache" });
            headers.Add(new Api.Rest.Header() { Name = "Host", Value = $"{_authRepository.Subdomain}.soap.marketingcloudapis.com" });
            return headers;
        }
    }
}