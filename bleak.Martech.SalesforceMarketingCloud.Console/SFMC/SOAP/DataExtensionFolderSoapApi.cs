using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Configuration;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Authentication;
using bleak.Martech.SalesforceMarketingCloud.ContentBuilder;
using bleak.Martech.SalesforceMarketingCloud.ContentBuilder.SfmcPocos;
using bleak.Martech.SalesforceMarketingCloud.Wsdl;
using System.Text;
using System.Security.Cryptography.Pkcs;

namespace bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Sfmc.Soap
{
    public partial class DataExtensionFolderSoapApi : BaseSoapApi
    {

        public DataExtensionFolderSoapApi(AuthRepository authRepository) : base(authRepository)
        {
        }

        public List<DataExtensionFolder> GetFolderTree()
        {
            int page = 1;
            int currentPageSize = 0;

            var wsdlFolders = new List<Wsdl.DataFolder>();
            string requestId = string.Empty;
            do
            {
                if (AppConfiguration.Instance.Debug) Console.WriteLine($"Loading Data Extension Folder Page {page}");

                requestId = LoadFolder(wsdlFolders, requestId);
                page++;
            }
            while (AppConfiguration.Instance.PageSize == currentPageSize);

            if (wsdlFolders.Any())
            {
                return BuildFolderTree(wsdlFolders);
            }

            throw new Exception("Error Loading Folders");
        }

        private string LoadFolder(List<Wsdl.DataFolder> wsdlFolders, string requestId = "")
        {
            try
            {
                if (AppConfiguration.Instance.Debug) { Console.WriteLine($"Invoking SOAP Call. URL: {url}"); }

                var results = _restManager.ExecuteRestMethod<SoapEnvelope<Wsdl.DataFolder>, string>(
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
                    wsdlFolders.Add(result);
                    currentPageSize++;
                }
                if (AppConfiguration.Instance.Debug) Console.WriteLine($"Current Page had {currentPageSize} records. There are now {wsdlFolders.Count()} Total Folders Identified.");

                if (results.Results.Body.RetrieveResponse.OverallStatus == "MoreDataAvailable")
                {
                    Console.WriteLine($"More Data Available. Request ID: {results.Results.Body.RetrieveResponse.RequestID}");
                    var retval = LoadFolder(wsdlFolders, results.Results.Body.RetrieveResponse.RequestID);
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
            /*
                            var envelope = new Envelope();

                            envelope.Header = new Soap.DataExtensions.Pocos.Header()
                            {
                                Action = new Soap.DataExtensions.Pocos.Action() { //MustUnderstand=1, 
                                    Value="Retrieve" },
                                //To = new To() { MustUnderstand=1, Value="https://mckg4sgmm8lcgdkc-h38b94l2bz0.soap.marketingcloudapis.com/Service.asmx" },
                                FuelOAuth = $"{_authRepository.Token.access_token}"
                            };

                            envelope.Body = new Soap.DataExtensions.Pocos.Body()
                            {
                                RetrieveRequestMsg = new RetrieveRequestMsg()
                                {
                                    RetrieveRequest = new Soap.DataExtensions.Pocos.RetrieveRequest()
                                    {
                                        ObjectType = "DataFolder",
                                        Properties = new string[] { "ObjectID", "ParentFolder.ID", "Name", "Description", "ContentType", "IsActive", "IsEditable" },
                                        Filter = new Soap.DataExtensions.Pocos.Filter()
                                        { 
                                            Property = "ContentType",
                                            SimpleOperator = "equals",
                                            Value   = "dataextension",
                                        }
                                    }
                                }
                            };
                            */

            //envelope.XmlnsS="http://www.w3.org/2003/05/soap-envelope";
            //envelope.XmlnsA="http://schemas.xmlsoap.org/ws/2004/08/addressing";
            //envelope.XmlnsU="http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd";

            /*results = ExecuteRestMethodWithRetry(
                loadFolderApiCall: LoadFolderApiCall,
                url: url,
                authenticationError: "401", 
                resolveAuthentication: _authRepository.ResolveAuthentication
            );*/

            /*if (!string.IsNullOrEmpty(requestId))
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"<s:Envelope xmlns:s=\"http://www.w3.org/2003/05/soap-envelope\" xmlns:a=\"http://schemas.xmlsoap.org/ws/2004/08/addressing\" xmlns:u=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\">");
                sb.AppendLine($"    <s:Header>");
                sb.AppendLine($"        <a:Action s:mustUnderstand=\"1\">Retrieve</a:Action>");
                sb.AppendLine($"        <a:To s:mustUnderstand=\"1\">https://{AppConfiguration.Instance.Subdomain}.soap.marketingcloudapis.com/Service.asmx</a:To>");
                sb.AppendLine($"        <fueloauth xmlns=\"http://exacttarget.com\">{_authRepository.Token.access_token}</fueloauth>");
                sb.AppendLine($"    </s:Header>");
                sb.AppendLine($"    <s:Body xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">");
                sb.AppendLine($"        <RetrieveRequestMsg xmlns=\"http://exacttarget.com/wsdl/partnerAPI\">");
                sb.AppendLine($"            <RetrieveRequest>");
                sb.AppendLine($"                <ContinueRequest>{requestId}</ContinueRequest>");
                sb.AppendLine($"                <ObjectType>DataFolder</ObjectType>");
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
                sb.AppendLine($"                    <Value>dataextension</Value>");
                sb.AppendLine($"                </Filter>");
                sb.AppendLine($"            </RetrieveRequest>");
                sb.AppendLine($"        </RetrieveRequestMsg>");
                sb.AppendLine($"    </s:Body>");
                sb.AppendLine($"</s:Envelope>");
                return sb;
            }
            else
            {*/
            var sb = new StringBuilder();
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
            sb.AppendLine($"                    <Value>dataextension</Value>");
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
}