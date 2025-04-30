using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Configuration;
using bleak.Martech.SalesforceMarketingCloud.Authentication;
using bleak.Martech.SalesforceMarketingCloud.Models;
using bleak.Martech.SalesforceMarketingCloud.Models.SfmcDtos;

using bleak.Martech.SalesforceMarketingCloud.Wsdl;
using System.Text;
using System.Security.Cryptography.Pkcs;

namespace bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Sfmc.Soap
{
    public partial class QueryDefinitionSoapApi : BaseSoapApi
    {
        public QueryDefinitionSoapApi(AuthRepository authRepository) : base(authRepository)
        {
        }
        
        public List<QueryDefinitionPoco> GetQueryDefinitionPocos()
        {
            int page = 1;
            int currentPageSize = 0;

            var wsdls = new List<Wsdl.QueryDefinition>();
            string requestId = string.Empty;
            do
            {
                if (AppConfiguration.Instance.Debug) Console.WriteLine($"Loading QueryDefinition {page}");
                requestId = LoadDataSet(wsdls, requestId);
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
                return pocos;
            }

            throw new Exception("Error Loading Folders");
        }

        private string LoadDataSet(List<Wsdl.QueryDefinition> wsdlDataExtensions, string requestId = "")
        {
            try
            {
                if (AppConfiguration.Instance.Debug) { Console.WriteLine($"Invoking SOAP Call. URL: {url}"); }

                var results = _restManager.ExecuteRestMethod<SoapEnvelope<Wsdl.QueryDefinition>, string>(
                    uri: new Uri(url),
                    verb: HttpVerbs.POST,
                    serializedPayload: BuildRequest(requestId).ToString(),
                    headers: BuildHeaders()
                );

                if (AppConfiguration.Instance.Debug) Console.WriteLine($"results.Value = {results?.Results}");
                if (results?.Error != null) Console.WriteLine($"results.Error = {results.Error}");

                // Process Results
                Console.WriteLine($"Overall Status: {results!.Results.Body.RetrieveResponse.OverallStatus}");
                int currentPageSize = 0;
                foreach (var result in results.Results.Body.RetrieveResponse.Results)
                {
                    wsdlDataExtensions.Add(result);
                    currentPageSize++;
                }
                if (AppConfiguration.Instance.Debug) Console.WriteLine($"Current Page had {currentPageSize} records. There are now {wsdlDataExtensions.Count()} Total Data Extensions Identified.");

                if (results.Results.Body.RetrieveResponse.OverallStatus == "MoreDataAvailable")
                {
                    Console.WriteLine($"More DataExtensions Available. Request ID: {results.Results.Body.RetrieveResponse.RequestID}");
                    var retval = LoadDataSet(wsdlDataExtensions, results.Results.Body.RetrieveResponse.RequestID);
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


        private StringBuilder BuildRequest(string requestId)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.AppendLine($"<s:Envelope xmlns:s=\"http://www.w3.org/2003/05/soap-envelope\" xmlns:a=\"http://schemas.xmlsoap.org/ws/2004/08/addressing\" xmlns:u=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\">");
            sb.AppendLine($"    <s:Header>");
            sb.AppendLine($"        <a:Action s:mustUnderstand=\"1\">Retrieve</a:Action>");
            sb.AppendLine($"        <a:To s:mustUnderstand=\"1\">https://{AppConfiguration.Instance.Subdomain}.soap.marketingcloudapis.com/Service.asmx</a:To>");
            sb.AppendLine($"        <fueloauth xmlns=\"http://exacttarget.com\">{_authRepository.Token.access_token}</fueloauth>");
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
            return sb;
        }
    }
}
