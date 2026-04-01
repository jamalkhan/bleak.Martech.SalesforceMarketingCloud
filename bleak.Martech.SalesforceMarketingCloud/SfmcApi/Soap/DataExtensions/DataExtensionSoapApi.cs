using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.Configuration;
using bleak.Martech.SalesforceMarketingCloud.Authentication;
using System.Text;
using bleak.Martech.SalesforceMarketingCloud.Api;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Sfmc.Soap;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using bleak.Martech.SalesforceMarketingCloud.Models.Pocos;
using bleak.Martech.SalesforceMarketingCloud.Models.Sfmc.Soap;
using System.Security;
using System.Xml.Linq;

namespace bleak.Martech.SalesforceMarketingCloud.Api.Soap;

public partial class DataExtensionSoapApi
:
    BaseSoapApi
    <
        DataExtensionSoapApi
    >,  IDataExtensionApi
{
    private const int RowImportChunkSize = 250;

    public DataExtensionSoapApi
    (
        IRestClientAsync restClientAsync,
        IAuthRepository authRepository,
        SfmcConnectionConfiguration config,
        ILogger<DataExtensionSoapApi> logger
    )
    : base
    (
        restClientAsync: restClientAsync,
        authRepository: authRepository,
        sfmcConnectionConfiguration: config,
        logger: logger
    )
    {
    }


    public async Task<List<DataExtensionPoco>> GetDataExtensionsByFolderAsync(int folderId)
    {
        _logger.LogInformation("Loading data extensions for folder {FolderId}.", folderId);
        var requestPayload = await BuildRequestAsync(folderId: folderId);
        return await IterateAPICallsForRequestAsync(requestPayload: requestPayload);
    }

    public async Task<List<DataExtensionPoco>> GetDataExtensionsNameLikeAsync(string nameLike)
    {
        _logger.LogInformation("Searching data extensions with LIKE filter. SearchTerm={SearchTerm}", nameLike);
        var requestPayload = await BuildRequestAsync(nameLike: nameLike);
        return await IterateAPICallsForRequestAsync(requestPayload: requestPayload);
    }

    public async Task<List<DataExtensionPoco>> GetDataExtensionsNameStartsWithAsync(string nameStartsWith)
    {
        _logger.LogInformation("Searching data extensions with starts-with filter. SearchTerm={SearchTerm}", nameStartsWith);
        var requestPayload = await BuildRequestAsync(nameStartsWith: nameStartsWith);
        return await IterateAPICallsForRequestAsync(requestPayload: requestPayload);
    }

    public async Task<List<DataExtensionPoco>> GetDataExtensionsNameEndsWithAsync(string nameEndsWith)
    {
        _logger.LogInformation("Searching data extensions with ends-with filter. SearchTerm={SearchTerm}", nameEndsWith);
        var requestPayload = await BuildRequestAsync(nameEndsWith: nameEndsWith);
        return await IterateAPICallsForRequestAsync(requestPayload: requestPayload);
    }

    public async Task<List<DataExtensionPoco>> GetAllDataExtensionsAsync()
    {
        _logger.LogInformation("Loading all data extensions.");
        var requestPayload = await BuildRequestAsync();
        return await IterateAPICallsForRequestAsync(requestPayload: requestPayload);
    }

    public async Task CreateDataExtensionAsync(DataExtensionImportDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        if (string.IsNullOrWhiteSpace(definition.Name))
            throw new ArgumentException("Data extension name is required.", nameof(definition));

        if (string.IsNullOrWhiteSpace(definition.CustomerKey))
            throw new ArgumentException("Customer key is required.", nameof(definition));

        if (definition.CategoryId <= 0)
            throw new ArgumentException("A target folder is required.", nameof(definition));

        if (definition.Columns.Count == 0)
            throw new ArgumentException("At least one column is required.", nameof(definition));

        _logger.LogInformation("Creating data extension. Name={Name}, CustomerKey={CustomerKey}, CategoryId={CategoryId}, ColumnCount={ColumnCount}", definition.Name, definition.CustomerKey, definition.CategoryId, definition.Columns.Count);
        var payload = await BuildCreateDataExtensionRequestAsync(definition);
        var responseXml = await ExecuteCreateRequestAsync(payload);
        EnsureCreateSucceeded(responseXml, "create data extension");
        _logger.LogInformation("Created data extension successfully. CustomerKey={CustomerKey}", definition.CustomerKey);
    }

    public async Task<int> AddRowsToDataExtensionAsync(string customerKey, IReadOnlyList<Dictionary<string, string>> rows)
    {
        if (string.IsNullOrWhiteSpace(customerKey))
            throw new ArgumentException("Customer key is required.", nameof(customerKey));

        if (rows.Count == 0)
            return 0;

        var imported = 0;
        _logger.LogInformation("Adding rows to data extension. CustomerKey={CustomerKey}, RowCount={RowCount}", customerKey, rows.Count);

        foreach (var chunk in rows.Chunk(RowImportChunkSize))
        {
            _logger.LogDebug("Importing data extension row chunk. CustomerKey={CustomerKey}, ChunkSize={ChunkSize}", customerKey, chunk.Length);
            var payload = await BuildCreateRowsRequestAsync(customerKey, chunk);
            var responseXml = await ExecuteCreateRequestAsync(payload);
            EnsureCreateSucceeded(responseXml, "insert data extension rows");
            imported += chunk.Length;
        }

        _logger.LogInformation("Completed row import. CustomerKey={CustomerKey}, ImportedRows={ImportedRows}", customerKey, imported);
        return imported;
    }

    private async Task<List<DataExtensionPoco>> IterateAPICallsForRequestAsync(string requestPayload)
    {
        int page = 1;
        int currentPageSize = 0;

        var wsdlDataExtensions = new List<Wsdl.DataExtension>();
        string requestId = string.Empty;
        do
        {
            _logger.LogDebug("Loading data extension SOAP page {PageNumber}.", page);
            requestId = await MakeApiCallAsync(wsdlDataExtensions, requestPayload);
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
            _logger.LogInformation("Data extension SOAP load completed. ResultCount={ResultCount}", dataExtensions.Count);
            return dataExtensions;
        }

        _logger.LogWarning("Data extension SOAP load returned no results.");
        throw new Exception("Error Loading Folders");
    }

    private async Task<string> MakeApiCallAsync(List<Wsdl.DataExtension> wsdlDataExtensions, string requestPayload)
    {
        try
        {
            _logger.LogDebug("Invoking data extension SOAP call. Url={Url}", url);
            _logger.LogTrace("Data extension SOAP payload: {Payload}", RedactSoapPayload(requestPayload));

            var results = await _restClientAsync.ExecuteRestMethodAsync<SoapEnvelope<Wsdl.DataExtension>, string>(
                uri: new Uri(url),
                verb: HttpVerbs.POST,
                serializedPayload: requestPayload,
                headers: BuildHeaders()
            );

            _logger.LogDebug("Data extension SOAP call completed. HasError={HasError}", !string.IsNullOrWhiteSpace(results?.Error));
            if (results?.Error != null) _logger.LogError("Data extension SOAP call returned an error. Error={Error}", results.Error);

            // Process Results
            _logger.LogInformation("Data extension SOAP response status: {OverallStatus}", results!.Results.Body.RetrieveResponse.OverallStatus);
            int currentPageSize = 0;
            foreach (var result in results.Results.Body.RetrieveResponse.Results)
            {
                wsdlDataExtensions.Add(result);
                currentPageSize++;
            }
            _logger.LogDebug("Data extension SOAP page processed. PageRecords={PageRecords}, AggregateRecords={AggregateRecords}", currentPageSize, wsdlDataExtensions.Count);

            if (results.Results.Body.RetrieveResponse.OverallStatus == "MoreDataAvailable")
            {
                _logger.LogWarning("More data extensions available. ContinueRequest={RequestId}", results.Results.Body.RetrieveResponse.RequestID);
                var moreDataRequestPayload = await BuildRequestAsync(requestId: results.Results.Body.RetrieveResponse.RequestID);
                var retval = await MakeApiCallAsync(wsdlDataExtensions, moreDataRequestPayload.ToString());
                return retval;

            }
            return string.Empty;
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Data extension SOAP call failed.");
            throw;
        }
    }

    private async Task<string> ExecuteCreateRequestAsync(string requestPayload)
    {
        _logger.LogDebug("Invoking SOAP create request. Url={Url}", url);
        _logger.LogTrace("SOAP create payload: {Payload}", RedactSoapPayload(requestPayload));
        var rawClient = new RestClient();
        var results = await rawClient.ExecuteRestMethodAsync<string, string>(
            uri: new Uri(url),
            verb: HttpVerbs.POST,
            serializedPayload: requestPayload,
            headers: BuildHeaders()
        );

        if (!string.IsNullOrWhiteSpace(results?.Error))
        {
            _logger.LogError("SOAP create request returned an error. Error={Error}", results.Error);
            throw new Exception(results.Error);
        }

        _logger.LogTrace("SOAP create response received. ResponseLength={ResponseLength}", results?.Results?.Length ?? 0);
        return results?.Results ?? throw new InvalidOperationException("SOAP create call returned no response.");
    }

    private async Task<string> BuildCreateDataExtensionRequestAsync(DataExtensionImportDefinition definition)
    {
        var token = await _authRepository.GetTokenAsync();
        var sb = new StringBuilder();

        sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.AppendLine("<s:Envelope xmlns:s=\"http://www.w3.org/2003/05/soap-envelope\" xmlns:a=\"http://schemas.xmlsoap.org/ws/2004/08/addressing\" xmlns:u=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\">");
        sb.AppendLine("    <s:Header>");
        sb.AppendLine("        <a:Action s:mustUnderstand=\"1\">Create</a:Action>");
        sb.AppendLine($"        <a:To s:mustUnderstand=\"1\">{soapToAddress}</a:To>");
        sb.AppendLine($"        <fueloauth xmlns=\"http://exacttarget.com\">{Escape(token.access_token)}</fueloauth>");
        sb.AppendLine("    </s:Header>");
        sb.AppendLine("    <s:Body xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">");
        sb.AppendLine("        <CreateRequest xmlns=\"http://exacttarget.com/wsdl/partnerAPI\">");
        sb.AppendLine("            <Objects xsi:type=\"DataExtension\">");
        sb.AppendLine($"                <CustomerKey>{Escape(definition.CustomerKey)}</CustomerKey>");
        sb.AppendLine($"                <Name>{Escape(definition.Name)}</Name>");
        sb.AppendLine($"                <Description>{Escape(definition.Description)}</Description>");
        sb.AppendLine($"                <CategoryID>{definition.CategoryId}</CategoryID>");
        sb.AppendLine("                <Fields>");

        foreach (var (column, index) in definition.Columns.Select((column, index) => (column, index)))
        {
            sb.AppendLine("                    <Field>");
            sb.AppendLine($"                        <Name>{Escape(column.Name)}</Name>");
            sb.AppendLine($"                        <FieldType>{MapFieldType(column.DataType)}</FieldType>");
            sb.AppendLine($"                        <IsRequired>{(!column.IsNullable).ToString().ToLowerInvariant()}</IsRequired>");
            sb.AppendLine($"                        <IsPrimaryKey>false</IsPrimaryKey>");
            sb.AppendLine($"                        <Ordinal>{index + 1}</Ordinal>");

            if (MapFieldType(column.DataType) == "Text")
            {
                var maxLength = Math.Clamp(column.MaxLength ?? 256, 1, 4000);
                sb.AppendLine($"                        <MaxLength>{maxLength}</MaxLength>");
            }

            sb.AppendLine("                    </Field>");
        }

        sb.AppendLine("                </Fields>");
        sb.AppendLine("            </Objects>");
        sb.AppendLine("        </CreateRequest>");
        sb.AppendLine("    </s:Body>");
        sb.AppendLine("</s:Envelope>");

        return sb.ToString();
    }

    private async Task<string> BuildCreateRowsRequestAsync(string customerKey, IReadOnlyList<Dictionary<string, string>> rows)
    {
        var token = await _authRepository.GetTokenAsync();
        var sb = new StringBuilder();

        sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.AppendLine("<s:Envelope xmlns:s=\"http://www.w3.org/2003/05/soap-envelope\" xmlns:a=\"http://schemas.xmlsoap.org/ws/2004/08/addressing\" xmlns:u=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\">");
        sb.AppendLine("    <s:Header>");
        sb.AppendLine("        <a:Action s:mustUnderstand=\"1\">Create</a:Action>");
        sb.AppendLine($"        <a:To s:mustUnderstand=\"1\">{soapToAddress}</a:To>");
        sb.AppendLine($"        <fueloauth xmlns=\"http://exacttarget.com\">{Escape(token.access_token)}</fueloauth>");
        sb.AppendLine("    </s:Header>");
        sb.AppendLine("    <s:Body xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">");
        sb.AppendLine("        <CreateRequest xmlns=\"http://exacttarget.com/wsdl/partnerAPI\">");

        foreach (var row in rows)
        {
            sb.AppendLine("            <Objects xsi:type=\"DataExtensionObject\">");
            sb.AppendLine($"                <CustomerKey>{Escape(customerKey)}</CustomerKey>");
            sb.AppendLine("                <Properties>");

            foreach (var kvp in row.Where(kvp => !string.IsNullOrWhiteSpace(kvp.Value)))
            {
                sb.AppendLine("                    <Property>");
                sb.AppendLine($"                        <Name>{Escape(kvp.Key)}</Name>");
                sb.AppendLine($"                        <Value>{Escape(kvp.Value)}</Value>");
                sb.AppendLine("                    </Property>");
            }

            sb.AppendLine("                </Properties>");
            sb.AppendLine("            </Objects>");
        }

        sb.AppendLine("        </CreateRequest>");
        sb.AppendLine("    </s:Body>");
        sb.AppendLine("</s:Envelope>");

        return sb.ToString();
    }

    private static void EnsureCreateSucceeded(string responseXml, string operationName)
    {
        var document = XDocument.Parse(responseXml);
        var ns = XNamespace.Get("http://exacttarget.com/wsdl/partnerAPI");

        var overallStatus = document.Descendants(ns + "OverallStatus").FirstOrDefault()?.Value ?? string.Empty;
        if (string.Equals(overallStatus, "OK", StringComparison.OrdinalIgnoreCase))
            return;

        var messages = document
            .Descendants(ns + "StatusMessage")
            .Select(x => x.Value?.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .ToList();

        var details = messages.Count > 0
            ? string.Join(" | ", messages)
            : $"SOAP create call returned status '{overallStatus}'.";

        throw new InvalidOperationException($"Failed to {operationName}. {details}");
    }

    private static string MapFieldType(string dataType)
        => dataType.ToLowerInvariant() switch
        {
            "int" => "Number",
            "float" => "Decimal",
            "datetime" => "Date",
            "bool" => "Boolean",
            _ => "Text",
        };

    private static string Escape(string? value)
        => SecurityElement.Escape(value) ?? string.Empty;


    private async Task<string> BuildRequestAsync(
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

        var token = await _authRepository.GetTokenAsync();
        var sb = new StringBuilder();
        sb.AppendLine($"<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.AppendLine($"<s:Envelope xmlns:s=\"http://www.w3.org/2003/05/soap-envelope\" xmlns:a=\"http://schemas.xmlsoap.org/ws/2004/08/addressing\" xmlns:u=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\">");
        sb.AppendLine($"    <s:Header>");
        sb.AppendLine($"        <a:Action s:mustUnderstand=\"1\">Retrieve</a:Action>");
        sb.AppendLine($"        <a:To s:mustUnderstand=\"1\">{soapToAddress}</a:To>");
        sb.AppendLine($"        <fueloauth xmlns=\"http://exacttarget.com\">{token?.access_token}</fueloauth>");
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
