using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.Authentication;
using bleak.Martech.SalesforceMarketingCloud.Configuration;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Sfmc.Soap;

namespace bleak.Martech.SalesforceMarketingCloud.Api.Soap;

public abstract partial class BaseSoapApi
{
    protected string url { get; private set;}
    protected IAuthRepository _authRepository { get; private set;}
    protected RestManager _restManager { get; private set;}
    protected SfmcConnectionConfiguration _sfmcConnectionConfiguration { get; private set;}

    public BaseSoapApi(
        IAuthRepository authRepository, 
        SfmcConnectionConfiguration sfmcConnectionConfiguration)
    {
        _sfmcConnectionConfiguration = sfmcConnectionConfiguration;
        _authRepository = authRepository;
        var soapSerializer = new SoapSerializer();
        _restManager = new RestManager(soapSerializer, soapSerializer);
        url = $"https://{_authRepository.Subdomain}.soap.marketingcloudapis.com/Service.asmx";
    }
    
    protected List<Header> BuildHeaders()
    {
        var headers = new List<Header>();
        headers.Add(new Header() { Name = "Content-Type", Value = "text/xml" });
        headers.Add(new Header() { Name = "Accept", Value = "/" });
        headers.Add(new Header() { Name = "Cache-Control", Value = "no-cache" });
        headers.Add(new Header() { Name = "Host", Value = $"{_authRepository.Subdomain}.soap.marketingcloudapis.com" });
        return headers;
    }
}
