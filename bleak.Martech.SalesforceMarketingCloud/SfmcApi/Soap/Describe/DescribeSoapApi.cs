using System.Text;
using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.Authentication;
using bleak.Martech.SalesforceMarketingCloud.Configuration;
using bleak.Martech.SalesforceMarketingCloud.Models.Sfmc.Soap;
using Microsoft.Extensions.Logging;

namespace bleak.Martech.SalesforceMarketingCloud.Api.Soap;

public interface IDescribeSoapApi
{

}
public partial class DescribeSoapApi
{

    protected IAuthRepository _authRepository { get; private set;}
    protected IRestClientAsync _restClientAsync { get; private set;}
    protected string url { get; private set; }
    protected readonly ILogger<DescribeSoapApi> _logger;
    protected SfmcConnectionConfiguration _sfmcConnectionConfiguration { get; private set; }
    public DescribeSoapApi
    (
        IRestClientAsync restClientAsync,
        IAuthRepository authRepository,
        SfmcConnectionConfiguration config,
        ILogger<DescribeSoapApi> logger
    )
    {
        _sfmcConnectionConfiguration = config;
        _authRepository = authRepository;
        _restClientAsync = restClientAsync;
        _logger = logger;
        _logger.LogInformation("DescribeSoapApi initialized.");
        url = Configuration.SfmcEndpointUrls.GetSoapServiceUrl(_authRepository.Subdomain, _sfmcConnectionConfiguration.SoapBaseUrl);
    }

    protected List<Header> BuildHeaders()
    {
        var headers = new List<Header>();
        headers.Add(new Header() { Name = "Content-Type", Value = "text/xml" });
        headers.Add(new Header() { Name = "Accept", Value = "/" });
        headers.Add(new Header() { Name = "Cache-Control", Value = "no-cache" });
        headers.Add(new Header() { Name = "Host", Value = Configuration.SfmcEndpointUrls.GetSoapHost(_authRepository.Subdomain, _sfmcConnectionConfiguration.SoapBaseUrl) });
        return headers;
    }


    public async Task<string> GetObjectDefinitionAsync(string objectType)
    {
        _logger.LogInformation("Describing SFMC object type {ObjectType}.", objectType);
        var requestPayload = await BuildRequestAsync(objectType: objectType);
        return await MakeApiCallAsync(requestPayload);
    }

    private async Task<string> MakeApiCallAsync(string requestPayload)
    {
        try
        {
            _logger.LogDebug("Invoking describe SOAP call. Url={Url}", url);
            _logger.LogTrace("Describe SOAP payload: {Payload}", BaseSoapApi<DescribeSoapApi>.RedactSoapPayload(requestPayload));

            //var results = await _restClientAsync.ExecuteRestMethodAsync<SoapEnvelope<ObjectDefinition>, string>
            var results = await _restClientAsync.ExecuteRestMethodAsync<string, string>
            (
                uri: new Uri(url),
                verb: HttpVerbs.POST,
                serializedPayload: requestPayload,
                headers: BuildHeaders()
            );

            _logger.LogDebug("Describe SOAP call completed. HasResults={HasResults}, HasError={HasError}", !string.IsNullOrWhiteSpace(results?.Results), !string.IsNullOrWhiteSpace(results?.Error));
            if (!string.IsNullOrWhiteSpace(results?.Error))
            {
                _logger.LogError("Describe SOAP call returned an error. Error={Error}", results.Error);
            }

            // Process Results
            //_logger.LogInformation($"Overall Status: {results!.Results.Body.RetrieveResponse.OverallStatus}");

            //return results.Results.Body.RetrieveResponse.Results.FirstOrDefault();
            _logger.LogInformation("Describe SOAP call completed successfully.");
            return results?.Results ?? string.Empty;
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Describe SOAP call failed.");
            throw;
        }
    }


    private async Task<string> BuildRequestAsync
    (
        string objectType
    )
    {

        var token = await _authRepository.GetTokenAsync();
        var sb = new StringBuilder();

        sb.AppendLine($"<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.AppendLine($"<s:Envelope xmlns:s=\"http://www.w3.org/2003/05/soap-envelope\" xmlns:a=\"http://schemas.xmlsoap.org/ws/2004/08/addressing\" xmlns:u=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\">");
        sb.AppendLine($"    <s:Header>");
        sb.AppendLine($"        <a:Action s:mustUnderstand=\"1\">Describe</a:Action>");
        sb.AppendLine($"        <a:To s:mustUnderstand=\"1\">{Configuration.SfmcEndpointUrls.GetSoapToAddress(_authRepository.Subdomain, _sfmcConnectionConfiguration.SoapBaseUrl)}</a:To>");
        sb.AppendLine($"        <fueloauth xmlns=\"http://exacttarget.com\">{token?.access_token}</fueloauth>");
        sb.AppendLine($"    </s:Header>");
        sb.AppendLine($"    <s:Body xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">");
        sb.AppendLine($"        <DefinitionRequestMsg xmlns=\"http://exacttarget.com/wsdl/partnerAPI\">");
        sb.AppendLine($"            <DescribeRequests>");
        sb.AppendLine($"                <ObjectDefinitionRequest>");
        sb.AppendLine($"                    <ObjectType>{objectType}</ObjectType>");
        sb.AppendLine($"                </ObjectDefinitionRequest>");
        sb.AppendLine($"            </DescribeRequests>");
        sb.AppendLine($"        </DefinitionRequestMsg>");
        sb.AppendLine($"    </s:Body>");
        sb.AppendLine($"</s:Envelope>");
        return sb.ToString();
    }
}
