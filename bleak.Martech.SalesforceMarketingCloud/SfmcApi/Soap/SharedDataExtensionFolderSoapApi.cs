using bleak.Api.Rest;
using System.Text;
using bleak.Martech.SalesforceMarketingCloud.Authentication;
using bleak.Martech.SalesforceMarketingCloud.Configuration;
using bleak.Martech.SalesforceMarketingCloud.Api;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Sfmc.Soap;
using Microsoft.Extensions.Logging;

namespace bleak.Martech.SalesforceMarketingCloud.Api.Soap;
public partial class SharedDataExtensionFolderSoapApi
    : BaseSoapApi<SharedDataExtensionFolderSoapApi>
{

    public SharedDataExtensionFolderSoapApi
    (
        IRestClientAsync restClientAsync,
        IAuthRepository authRepository,
        SfmcConnectionConfiguration config,
        ILogger<SharedDataExtensionFolderSoapApi> logger
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

    public async Task<List<DataExtensionFolder>> GetFolderTreeAsync()
    {
        int page = 1;
        int currentPageSize = 0;

        var wsdlFolders = new List<Wsdl.DataFolder>();
        string requestId = string.Empty;
        do
        {
            _logger.LogInformation($"Loading Shared Data Extension Folder Page {page}");

            requestId = await LoadFolderAsync(wsdlFolders, requestId);
            page++;
        }
        while (_sfmcConnectionConfiguration.PageSize == currentPageSize);

        if (wsdlFolders.Any())
        {
            return BuildFolderTree(wsdlFolders);
        }

        throw new Exception("Error Loading Shared Data Extension Folders via Soap");
    }

    private async Task<string> LoadFolderAsync(List<Wsdl.DataFolder> wsdlFolders, string requestId = "")
    {
        try
        {
            _logger.LogInformation($"Invoking SOAP Call. URL: {url}");

            var results = await _restClientAsync.ExecuteRestMethodAsync<SoapEnvelope<Wsdl.DataFolder>, string>(
                uri: new Uri(url),
                verb: HttpVerbs.POST,
                serializedPayload: BuildRequest(requestId).ToString(),
                headers: BuildHeaders()
            );

            _logger.LogInformation($"results.Value = {results?.Results}");
            if (results?.Error != null) _logger.LogError($"results.Error = {results.Error}");

            // Process Results
            _logger.LogInformation($"Overall Status: {results!.Results.Body.RetrieveResponse.OverallStatus}");
            int currentPageSize = 0;
            foreach (var result in results.Results.Body.RetrieveResponse.Results)
            {
                wsdlFolders.Add(result);
                currentPageSize++;
            }
            _logger.LogInformation($"Current Page had {currentPageSize} records. There are now {wsdlFolders.Count()} Total Folders Identified.");

            if (results.Results.Body.RetrieveResponse.OverallStatus == "MoreDataAvailable")
            {
                _logger.LogInformation($"More Data Available. Request ID: {results.Results.Body.RetrieveResponse.RequestID}");
                var retval = await LoadFolderAsync(wsdlFolders, results.Results.Body.RetrieveResponse.RequestID);
                return retval;

            }
            return string.Empty;
        }
        catch (System.Exception ex)
        {
            _logger.LogError($"Error {ex.Message}");
            throw;
        }
    }

    private StringBuilder BuildRequest(string requestId)
    {
        var sb = new StringBuilder();
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
        sb.AppendLine($"                <ObjectType>DataFolder</ObjectType>");
        sb.AppendLine($"                <Properties>ID</Properties>");
        sb.AppendLine($"                <Properties>ObjectID</Properties>");
        sb.AppendLine($"                <Properties>ParentFolder.ID</Properties>");
        sb.AppendLine($"                <Properties>ParentFolder.Name</Properties>");
        sb.AppendLine($"                <Properties>Name</Properties>");
        sb.AppendLine($"                <Properties>Description</Properties>");
        sb.AppendLine($"                <Properties>ContentType</Properties>");
        sb.AppendLine($"                <Properties>IsActive</Properties>");
        sb.AppendLine($"                <Properties>IsEditable</Properties>");
        sb.AppendLine($"                <Filter xsi:type=\"SimpleFilterPart\">");
        sb.AppendLine($"                    <Property>ContentType</Property>");
        sb.AppendLine($"                    <SimpleOperator>equals</SimpleOperator>");
        sb.AppendLine($"                    <Value>shared_data</Value>");
        sb.AppendLine($"                </Filter>");
        sb.AppendLine($"            </RetrieveRequest>");
        sb.AppendLine($"        </RetrieveRequestMsg>");
        sb.AppendLine($"    </s:Body>");
        sb.AppendLine($"</s:Envelope>");
        return sb;

    }

    List<DataExtensionFolder> BuildFolderTree(List<Wsdl.DataFolder> wsdlFolders)
    {
        const int root_folder = 0;

        // Find root folders
        var wsdlFolderRoots = wsdlFolders.Where(f => f.ParentFolder.ID == root_folder).ToList();
        var retval = new List<DataExtensionFolder>();
        foreach (var wsdlFolder in wsdlFolderRoots)
        {
            var folder = wsdlFolder.ToDataExtensionFolder();
            folder.FullPath = $"/{folder.Name}";

            // TODO: Reimplement this
            // GetAssetsByFolder(folderObject);
            AddChildren(folder, wsdlFolders);
            retval.Add(folder);
        }
        return retval;
    }

    void AddChildren(DataExtensionFolder folderObject, List<Wsdl.DataFolder> wsdlFolders)
    {
        var children = wsdlFolders.Where(f => f.ParentFolder.ID == folderObject.Id).ToList();
        var childrenCount = children.Count;
        foreach (var child in children)
        {
            var childFolder = child.ToDataExtensionFolder();
            childFolder.FullPath = $"{folderObject.FullPath}/{childFolder.Name}";
            folderObject.SubFolders.Add(childFolder);
            AddChildren(childFolder, wsdlFolders);
        }
    }
}
