using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.Authentication;
using bleak.Martech.SalesforceMarketingCloud.Configuration;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Sfmc.Soap;
using Microsoft.Extensions.Logging;

namespace bleak.Martech.SalesforceMarketingCloud.Api.Soap;

public abstract partial class BaseSoapApi<T>
{
    protected readonly ILogger<T> _logger;
    protected string url { get; private set; }
    protected IAuthRepository _authRepository { get; private set;}
    protected IRestClientAsync _restClientAsync { get; private set;}
    protected SfmcConnectionConfiguration _sfmcConnectionConfiguration { get; private set; }

    public BaseSoapApi(
        IRestClientAsync restClientAsync,
        IAuthRepository authRepository, 
        SfmcConnectionConfiguration sfmcConnectionConfiguration,
        ILogger<T> logger)
    {
        _logger = logger;
        _logger.LogInformation("Logger provided to BaseSoapApi.");
    
        _sfmcConnectionConfiguration = sfmcConnectionConfiguration;
        _authRepository = authRepository;
        _restClientAsync = restClientAsync;
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
