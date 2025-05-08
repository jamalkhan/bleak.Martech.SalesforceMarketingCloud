using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.Configuration;
using bleak.Martech.SalesforceMarketingCloud.Authentication;
using System.Text;

namespace bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Sfmc.Soap
{
    public partial class DataExtensionSoapApi : BaseSoapApi
    {
        public DataExtensionSoapApi
        (
            IAuthRepository authRepository
        )
        : base
        (
            authRepository: authRepository,
            sfmcConnectionConfiguration: new SfmcConnectionConfiguration()
        )
        {
        }
        
        public List<DataExtensionPoco> GetAllDataExtensions()
        {
            int page = 1;
            int currentPageSize = 0;

            var wsdlDataExtensions = new List<Wsdl.DataExtension>();
            string requestId = string.Empty;
            do
            {
                if (_sfmcConnectionConfiguration.Debug) Console.WriteLine($"Loading Data Extension {page}");
                requestId = LoadDataExtensions(wsdlDataExtensions, requestId);
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

        private string LoadDataExtensions(List<Wsdl.DataExtension> wsdlDataExtensions, string requestId = "")
        {
            try
            {
                if (_sfmcConnectionConfiguration.Debug) { Console.WriteLine($"Invoking SOAP Call. URL: {url}"); }

                var results = _restManager.ExecuteRestMethod<SoapEnvelope<Wsdl.DataExtension>, string>(
                    uri: new Uri(url),
                    verb: HttpVerbs.POST,
                    serializedPayload: BuildRequest(requestId).ToString(),
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
                    var retval = LoadDataExtensions(wsdlDataExtensions, results.Results.Body.RetrieveResponse.RequestID);
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


        private string BuildRequest(string requestId)
        {
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
            sb.AppendLine($"            </RetrieveRequest>");
            sb.AppendLine($"        </RetrieveRequestMsg>");
            sb.AppendLine($"    </s:Body>");
            sb.AppendLine($"</s:Envelope>");
            return sb.ToString();
        }
    }
}
