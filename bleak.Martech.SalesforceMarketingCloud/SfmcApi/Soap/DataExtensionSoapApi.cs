using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.Configuration;
using bleak.Martech.SalesforceMarketingCloud.Authentication;
using System.Text;
using bleak.Martech.SalesforceMarketingCloud.Api;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Sfmc.Soap;
using Microsoft.Extensions.Logging;

namespace bleak.Martech.SalesforceMarketingCloud.Api.Soap;

public partial class DataExtensionSoapApi : BaseSoapApi<DataExtensionSoapApi>, IDataExtensionApi
{
    public DataExtensionSoapApi
    (
        IAuthRepository authRepository,
        SfmcConnectionConfiguration config,
        ILogger<DataExtensionSoapApi> logger
    )
    : base
    (
        authRepository: authRepository,
        sfmcConnectionConfiguration: config,
        logger: logger
    )
    {
    }


    public Task<List<DataExtensionPoco>> GetDataExtensionsByFolderAsync(int folderId)
    {
        return Task.Run(() => GetDataExtensionsByFolder(folderId));
    }
    private List<DataExtensionPoco> GetDataExtensionsByFolder(int folderId)
    {
        var requestPayload = BuildRequest(folderId: folderId);
        return IterateAPICallsForRequest(requestPayload: requestPayload);
    }


    public Task<List<DataExtensionPoco>> GetDataExtensionsNameLikeAsync(string nameLike)
    {
        return Task.Run(() => GetDataExtensionsNameLike(nameLike));
    }
    private List<DataExtensionPoco> GetDataExtensionsNameLike(string nameLike)
    {
        var requestPayload = BuildRequest(nameLike: nameLike);
        return IterateAPICallsForRequest(requestPayload: requestPayload);
    }

    public Task<List<DataExtensionPoco>> GetDataExtensionsNameStartsWithAsync(string nameStartsWith)
    {
        return Task.Run(() => GetDataExtensionsNameStartsWith(nameStartsWith));
    }
    private List<DataExtensionPoco> GetDataExtensionsNameStartsWith(string nameStartsWith)
    {
        var requestPayload = BuildRequest(nameStartsWith: nameStartsWith);
        return IterateAPICallsForRequest(requestPayload: requestPayload);
    }

    public Task<List<DataExtensionPoco>> GetDataExtensionsNameEndsWithAsync(string nameEndsWith)
    {
        return Task.Run(() => GetDataExtensionsNameEndsWith(nameEndsWith));
    }
    private List<DataExtensionPoco> GetDataExtensionsNameEndsWith(string nameEndsWith)
    {
        var requestPayload = BuildRequest(nameEndsWith: nameEndsWith);
        return IterateAPICallsForRequest(requestPayload: requestPayload);
    }

    public Task<List<DataExtensionPoco>> GetAllDataExtensionsAsync()
    {
        return Task.Run(() => GetAllDataExtensions());
    }
    public List<DataExtensionPoco> GetAllDataExtensions()
    {
        var requestPayload = BuildRequest();
        return IterateAPICallsForRequest(requestPayload: requestPayload);
    }

    private List<DataExtensionPoco> IterateAPICallsForRequest(string requestPayload)
    {
        int page = 1;
        int currentPageSize = 0;

        var wsdlDataExtensions = new List<Wsdl.DataExtension>();
        string requestId = string.Empty;
        do
        {
            if (_sfmcConnectionConfiguration.Debug) Console.WriteLine($"Loading Data Extension {page}");
            requestId = MakeApiCall(wsdlDataExtensions, requestPayload);
            page++;
        }
        while (_sfmcConnectionConfiguration.PageSize == currentPageSize);

        if (wsdlDataExtensions.Any())
        {
            var dataExtensions = new List<DataExtensionPoco>();
            foreach (var wsdlDataExtension in wsdlDataExtensions)
            {
                dataExtensions.Add(wsdlDataExtension.ToDataExtensionPoco());
            }
            return dataExtensions;
        }

        throw new Exception("Error Loading Folders");
    }

    private string MakeApiCall(List<Wsdl.DataExtension> wsdlDataExtensions, string requestPayload)
    {
        try
        {
            if (_sfmcConnectionConfiguration.Debug) { Console.WriteLine($"Invoking SOAP Call. URL: {url}"); }

            var results = _restManager.ExecuteRestMethod<SoapEnvelope<Wsdl.DataExtension>, string>(
                uri: new Uri(url),
                verb: HttpVerbs.POST,
                serializedPayload: requestPayload,
                headers: BuildHeaders()
            );

            if (_sfmcConnectionConfiguration.Debug) Console.WriteLine($"results.Value = {results?.Results}");
            if (results?.Error != null) Console.WriteLine($"results.Error = {results.Error}");

            // Process Results
            Console.WriteLine($"Overall Status: {results!.Results.Body.RetrieveResponse.OverallStatus}");
            int currentPageSize = 0;
            foreach (var result in results.Results.Body.RetrieveResponse.Results)
            {
                wsdlDataExtensions.Add(result);
                currentPageSize++;
            }
            if (_sfmcConnectionConfiguration.Debug) Console.WriteLine($"Current Page had {currentPageSize} records. There are now {wsdlDataExtensions.Count()} Total Data Extensions Identified.");

            if (results.Results.Body.RetrieveResponse.OverallStatus == "MoreDataAvailable")
            {
                Console.WriteLine($"More DataExtensions Available. Request ID: {results.Results.Body.RetrieveResponse.RequestID}");
                var moreDataRequestPayload = BuildRequest(requestId: results.Results.Body.RetrieveResponse.RequestID).ToString();
                var retval = MakeApiCall(wsdlDataExtensions, moreDataRequestPayload);
                return retval;

            }
            return string.Empty;
        }
        catch (System.Exception ex)
        {
            Console.WriteLine($"Error {ex.Message}");
            throw;
        }
    }


    private string BuildRequest(
        string? requestId = null,
        int? folderId = null,
        string? nameEndsWith = null,
        string? nameLike = null,
        string? nameStartsWith = null
        )
    {
        /*
        // TODO: Reimplement validation at a later time.
        if (string.IsNullOrEmpty(requestId) && !folderId.HasValue && string.IsNullOrEmpty(name))
        {
            throw new ArgumentException("Either requestId, folderId or name must be provided.");
        }
        if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(nameLike))
        {
            throw new ArgumentException("Either name or nameLike must be provided, not both.");
        }
    {
        if (!string.IsNullOrEmpty(requestId) && folderId.HasValue)
        {
            throw new ArgumentException("Either requestId or folderId must be provided, not both.");
        }
        */

        var sb = new StringBuilder();
        sb.AppendLine($"<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.AppendLine($"<s:Envelope xmlns:s=\"http://www.w3.org/2003/05/soap-envelope\" xmlns:a=\"http://schemas.xmlsoap.org/ws/2004/08/addressing\" xmlns:u=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\">");
        sb.AppendLine($"    <s:Header>");
        sb.AppendLine($"        <a:Action s:mustUnderstand=\"1\">Retrieve</a:Action>");
        sb.AppendLine($"        <a:To s:mustUnderstand=\"1\">https://{_authRepository.Subdomain}.soap.marketingcloudapis.com/Service.asmx</a:To>");
        sb.AppendLine($"        <fueloauth xmlns=\"http://exacttarget.com\">{_authRepository.Token.access_token}</fueloauth>");
        sb.AppendLine($"    </s:Header>");
        sb.AppendLine($"    <s:Body xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">");
        sb.AppendLine($"        <RetrieveRequestMsg xmlns=\"http://exacttarget.com/wsdl/partnerAPI\">");
        sb.AppendLine($"            <RetrieveRequest>");
        if (!string.IsNullOrEmpty(requestId))
        {
            sb.AppendLine($"                <ContinueRequest>{requestId}</ContinueRequest>");
        }
        sb.AppendLine($"                <ObjectType>DataExtension</ObjectType>");
        sb.AppendLine($"                <Properties>ObjectID</Properties>");
        sb.AppendLine($"                <Properties>CustomerKey</Properties>");
        sb.AppendLine($"                <Properties>Name</Properties>");
        sb.AppendLine($"                <Properties>Description</Properties>");
        sb.AppendLine($"                <Properties>CategoryID</Properties>");
        sb.AppendLine($"                <Properties>IsSendable</Properties>");
        sb.AppendLine($"                <Properties>IsTestable</Properties>");
        if (folderId.HasValue)
        {
            sb.AppendLine($"                <Filter xsi:type=\"SimpleFilterPart\">");
            sb.AppendLine($"                    <Property>CategoryID</Property>");
            sb.AppendLine($"                    <SimpleOperator>equals</SimpleOperator>");
            sb.AppendLine($"                    <Value>{folderId.Value}</Value>");
            sb.AppendLine($"                </Filter>");
        }



        if (!string.IsNullOrEmpty(nameStartsWith))
        {
            sb.AppendLine($"                <Filter xsi:type=\"SimpleFilterPart\">");
            sb.AppendLine($"                    <Property>Name</Property>");
            sb.AppendLine($"                    <SimpleOperator>startsWith</SimpleOperator>");
            sb.AppendLine($"                    <Value>{nameStartsWith}</Value>");
            sb.AppendLine($"                </Filter>");
        }
        else if (!string.IsNullOrEmpty(nameEndsWith))
        {
            sb.AppendLine($"                <Filter xsi:type=\"SimpleFilterPart\">");
            sb.AppendLine($"                    <Property>Name</Property>");
            sb.AppendLine($"                    <SimpleOperator>endsWith</SimpleOperator>");
            sb.AppendLine($"                    <Value>{nameEndsWith}</Value>");
            sb.AppendLine($"                </Filter>");
        }
        else if (!string.IsNullOrEmpty(nameLike))
        {
            /*
                <Filter xsi:type="ComplexFilterPart" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
                <LeftOperand xsi:type="SimpleFilterPart">
                    <Property>EmailAddress</Property>
                    <SimpleOperator>like</SimpleOperator>
                    <Value>%@example.com%</Value>
                </LeftOperand>
                <LogicalOperator>OR</LogicalOperator>
                <RightOperand xsi:type="SimpleFilterPart">
                    <Property>SubscriberKey</Property>
                    <SimpleOperator>like</SimpleOperator>
                    <Value>abc%</Value>
                </RightOperand>
                </Filter>
            */
            sb.AppendLine($"                <Filter xsi:type=\"ComplexFilterPart\">");
            sb.AppendLine($"                    <LeftOperand xsi:type=\"SimpleFilterPart\">");
            sb.AppendLine($"                        <Property>Name</Property>");
            sb.AppendLine($"                        <SimpleOperator>like</SimpleOperator>");
            sb.AppendLine($"                        <Value>{nameLike}</Value>");
            sb.AppendLine($"                    </LeftOperand>");
            sb.AppendLine($"                    <LogicalOperator>OR</LogicalOperator>");
            sb.AppendLine($"                    <RightOperand xsi:type=\"SimpleFilterPart\">");
            sb.AppendLine($"                        <Property>CustomerKey</Property>");
            sb.AppendLine($"                        <SimpleOperator>like</SimpleOperator>");
            sb.AppendLine($"                        <Value>{nameLike}</Value>");
            sb.AppendLine($"                    </RightOperand>");
            sb.AppendLine($"                </Filter>");
        }
        else
        {

        }

        sb.AppendLine($"            </RetrieveRequest>");
        sb.AppendLine($"        </RetrieveRequestMsg>");
        sb.AppendLine($"    </s:Body>");
        sb.AppendLine($"</s:Envelope>");
        return sb.ToString();
    }
}
