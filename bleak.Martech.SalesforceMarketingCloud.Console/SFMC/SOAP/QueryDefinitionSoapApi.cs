using System.Text;
using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.Api.Soap;
using bleak.Martech.SalesforceMarketingCloud.Authentication;
using bleak.Martech.SalesforceMarketingCloud.Configuration;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Configuration;
using bleak.Martech.SalesforceMarketingCloud.Models.Pocos;
using bleak.Martech.SalesforceMarketingCloud.Models.Sfmc.Soap;
using Microsoft.Extensions.Logging;

namespace bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Sfmc.Soap;

public partial class QueryDefinitionSoapApi : BaseSoapApi<QueryDefinitionSoapApi>
{
    public QueryDefinitionSoapApi
    (
        IRestClientAsync restClientAsync,
        IAuthRepository authRepository,
        ILogger<QueryDefinitionSoapApi> logger
    )
    : base
    (
        restClientAsync: restClientAsync,
        authRepository: authRepository,
        sfmcConnectionConfiguration: new SfmcConnectionConfiguration(),
        logger: logger
    )
    {
    }
    
    public async Task<List<QueryDefinitionPoco>> GetQueryDefinitionPocosAsync()
    {
        _logger.LogInformation("Loading query definitions from SFMC.");
        int page = 1;
        int currentPageSize = 0;

        var wsdls = new List<Wsdl.QueryDefinition>();
        string requestId = string.Empty;
        do
        {
            _logger.LogDebug("Loading query definition SOAP page {PageNumber}.", page);
            requestId = await LoadDataSetAsync(wsdls, requestId);
            page++;
        }
        while (AppConfiguration.Instance.PageSize == currentPageSize);

        if (wsdls.Any())
        {
            var pocos = new List<QueryDefinitionPoco>();
            foreach (var wsdl in wsdls)
            {
                pocos.Add(wsdl.ToPoco());
            }
            _logger.LogInformation("Loaded {QueryDefinitionCount} query definitions.", pocos.Count);
            return pocos;
        }

        _logger.LogWarning("Query definition SOAP load returned no results.");
        throw new Exception("Error Loading Folders");
    }

    private async Task<string> LoadDataSetAsync(List<Wsdl.QueryDefinition> wsdlDataExtensions, string requestId = "")
    {
        try
        {
            _logger.LogDebug("Invoking query definition SOAP call. Url={Url}", url);

            var payload = await BuildRequestAsync(requestId);
            _logger.LogTrace("Query definition SOAP payload: {Payload}", BaseSoapApi<QueryDefinitionSoapApi>.RedactSoapPayload(payload));
            var results = await _restClientAsync.ExecuteRestMethodAsync<SoapEnvelope<Wsdl.QueryDefinition>, string>(
                uri: new Uri(url),
                verb: HttpVerbs.POST,
                serializedPayload: payload,
                headers: BuildHeaders()
            );

            _logger.LogDebug("Query definition SOAP call completed. HasError={HasError}", !string.IsNullOrWhiteSpace(results?.Error));
            if (results?.Error != null) _logger.LogError("Query definition SOAP call returned an error. Error={Error}", results.Error);

            // Process Results
            _logger.LogInformation("Query definition SOAP response status: {OverallStatus}", results!.Results.Body.RetrieveResponse.OverallStatus);
            int currentPageSize = 0;
            foreach (var result in results.Results.Body.RetrieveResponse.Results)
            {
                wsdlDataExtensions.Add(result);
                currentPageSize++;
            }
            _logger.LogDebug("Query definition SOAP page processed. PageRecords={PageRecords}, AggregateRecords={AggregateRecords}", currentPageSize, wsdlDataExtensions.Count);

            if (results.Results.Body.RetrieveResponse.OverallStatus == "MoreDataAvailable")
            {
                _logger.LogWarning("More query definitions available. ContinueRequest={RequestId}", results.Results.Body.RetrieveResponse.RequestID);
                var retval = await LoadDataSetAsync(wsdlDataExtensions, results.Results.Body.RetrieveResponse.RequestID);
                return retval;
                
            }
            return string.Empty;
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Query definition SOAP call failed.");
            throw;
        }
    }


    private async Task<string> BuildRequestAsync(string requestId)
    {
        var token = await _authRepository.GetTokenAsync();
        var sb = new StringBuilder();
        sb.AppendLine($"<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.AppendLine($"<s:Envelope xmlns:s=\"http://www.w3.org/2003/05/soap-envelope\" xmlns:a=\"http://schemas.xmlsoap.org/ws/2004/08/addressing\" xmlns:u=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\">");
        sb.AppendLine($"    <s:Header>");
        sb.AppendLine($"        <a:Action s:mustUnderstand=\"1\">Retrieve</a:Action>");
        sb.AppendLine($"        <a:To s:mustUnderstand=\"1\">{soapToAddress}</a:To>");
        sb.AppendLine($"        <fueloauth xmlns=\"http://exacttarget.com\">{token.access_token}</fueloauth>");
        sb.AppendLine($"    </s:Header>");
        sb.AppendLine($"    <s:Body xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">");
        sb.AppendLine($"        <RetrieveRequestMsg xmlns=\"http://exacttarget.com/wsdl/partnerAPI\">");
        sb.AppendLine($"            <RetrieveRequest>");
        if (!string.IsNullOrEmpty(requestId))
        {
            sb.AppendLine($"                <ContinueRequest>{requestId}</ContinueRequest>");
        }
        sb.AppendLine($"                <ObjectType>QueryDefinition</ObjectType>");
        sb.AppendLine($"                <Properties>Name</Properties>");
        sb.AppendLine($"                <Properties>Description</Properties>");
        sb.AppendLine($"                <Properties>CustomerKey</Properties>");
        sb.AppendLine($"                <Properties>DataExtensionTarget.Name</Properties>");
        sb.AppendLine($"                <Properties>FileSpec</Properties>");
        sb.AppendLine($"                <Properties>FileType</Properties>");
        sb.AppendLine($"                <Properties>QueryText</Properties>");
        sb.AppendLine($"            </RetrieveRequest>");
        sb.AppendLine($"        </RetrieveRequestMsg>");
        sb.AppendLine($"    </s:Body>");
        sb.AppendLine($"</s:Envelope>");
        return sb.ToString();
    }
}
