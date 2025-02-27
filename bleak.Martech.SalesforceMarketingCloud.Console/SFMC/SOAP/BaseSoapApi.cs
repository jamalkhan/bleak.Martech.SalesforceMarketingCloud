using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Configuration;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Authentication;

namespace bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Sfmc.Soap
{
    public abstract partial class BaseSoapApi
    {
        protected string url = $"https://{AppConfiguration.Instance.Subdomain}.soap.marketingcloudapis.com/Service.asmx";
        protected AuthRepository _authRepository;
        protected RestManager _restManager;

        public BaseSoapApi(AuthRepository authRepository)
        {
            _authRepository = authRepository;
            var soapSerializer = new SoapSerializer();
            _restManager = new RestManager(soapSerializer, soapSerializer);
        }
        
        protected List<Header> BuildHeaders()
        {
            var headers = new List<Api.Rest.Header>();
            headers.Add(new Api.Rest.Header() { Name = "Content-Type", Value = "text/xml" });
            headers.Add(new Api.Rest.Header() { Name = "Accept", Value = "/" });
            headers.Add(new Api.Rest.Header() { Name = "Cache-Control", Value = "no-cache" });
            headers.Add(new Api.Rest.Header() { Name = "Host", Value = $"{AppConfiguration.Instance.Subdomain}.soap.marketingcloudapis.com" });
            return headers;
        }
    }
}