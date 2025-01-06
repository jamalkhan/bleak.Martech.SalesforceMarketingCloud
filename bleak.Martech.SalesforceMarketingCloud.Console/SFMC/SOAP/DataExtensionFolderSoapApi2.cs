using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Configuration;
using bleak.Martech.SalesforceMarketingCloud.Wsdl;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Sfmc.Soap.DataExtensions
{
    public partial class DataExtensionFolderSoapApi2
    {
        AuthRepository _authRepository;
        
        SoapClient<Wsdl.DataFolder> soapClient;

        public DataExtensionFolderSoapApi2(AuthRepository authRepository)
        {
            _authRepository = authRepository;
            var endpointAddress = new EndpointAddress($"https://{AppConfiguration.Instance.Subdomain}.soap.marketingcloudapis.com/Service.asmx");
            var binding = new BasicHttpBinding(BasicHttpSecurityMode.Transport);
            soapClient = new SoapClient<Wsdl.DataFolder>(binding, endpointAddress);
        }

        public string GetFolderTree()
        {
            
            // Assuming "SoapClient" is the generated class from the WSDL
            

            

            // Add the access token to the HTTP header
            using var scope = new OperationContextScope(soapClient.InnerChannel);
            var httpRequestProperty = new HttpRequestMessageProperty();
            httpRequestProperty.Headers["Authorization"] = "Bearer " + _authRepository.Token.access_token;
            OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;

            // Example SOAP request (e.g., retrieving data extensions)
            var request = new RetrieveRequest1
            {

                RetrieveRequest = new RetrieveRequest()
                {
                    ObjectType = "DataFolder",
                    Properties = new[] { "ID", "Name", "IsActive", "IsEditable", "ParentFolder" },
                }
            };


            var response = soapClient.RetrieveAsync(request);
            
            foreach (var item in response.Result.Results)
            {
                Console.WriteLine(item);
            }
            return "x";
/*
            int page = 1;
            int currentPageSize = 0;

            var sfmcFolders = new List<SfmcFolder>();
            do
            {
                if (AppConfiguration.Instance.Debug) Console.WriteLine($"Loading Data Extension Folder Page {page}");

                currentPageSize = LoadFolder(page, sfmcFolders);
                page++;
            }
            while (AppConfiguration.Instance.PageSize == currentPageSize);

            if (sfmcFolders.Any())
            {
                return BuildFolderTree(sfmcFolders);
            }

            throw new Exception("Error Loading Folders");
            */
        }

/*
        private int LoadFolder(int page, List<SfmcFolder> sfmcFolders)
        {
            int currentPageSize;
            try
            {
                
                
                RestResults<string, string> results;
                string url = $"https://{AppConfiguration.Instance.Subdomain}.soap.marketingcloudapis.com/Service.asmx";

                if (AppConfiguration.Instance.Debug) { Console.WriteLine($"Loading Folder Page #{page}, URL: {url}"); }

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
                /*
                if (AppConfiguration.Instance.Debug) { Console.WriteLine($"Invoking SOAP Call."); }
                results = _restManager.ExecuteRestMethod<string, string>(
                    uri: new Uri(url),
                    verb: HttpVerbs.POST, 
                    payload: envelope
                );

                if (AppConfiguration.Instance.Debug) Console.WriteLine($"results.Value = {results?.Results}");
                if (results?.Error != null) Console.WriteLine($"results.Error = {results.Error}");

                /*
                currentPageSize = results!.Results.items.Count();
                sfmcFolders.AddRange(results.Results.items);
                if (AppConfiguration.Instance.Debug) Console.WriteLine($"Current Page had {currentPageSize} records. There are now {sfmcFolders.Count()} Total Folders Identified.");

                if (AppConfiguration.Instance.PageSize == currentPageSize)
                {
                    if (AppConfiguration.Instance.Debug) Console.WriteLine($"Running Loop Again");
                }
                */
            /*}
            catch (System.Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                throw;
            }

            //return currentPageSize;
            return 0;
        }


        List<FolderObject> BuildFolderTree(List<SfmcFolder> sfmcFolders)
        {
            const int root_folder = 0;

            // Find root folders
            var sfmcRoots = sfmcFolders.Where(f => f.parentId == root_folder).ToList();
            var retval = new List<FolderObject>();
            foreach (var sfmcRoot in sfmcRoots)
            {
                var folderObject = sfmcRoot.ToFolderObject();
                folderObject.FullPath = "/";

                // TODO: Reimplement this
                // GetAssetsByFolder(folderObject);
                // AddChildren(folderObject, sfmcFolders);
                retval.Add(folderObject);
            }
            return retval;
        }*/
    }

}

    /*

Request
<s:Envelope xmlns:s="http://www.w3.org/2003/05/soap-envelope" xmlns:a="http://schemas.xmlsoap.org/ws/2004/08/addressing" xmlns:u="http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd">
    <s:Header>
        <a:Action s:mustUnderstand="1">Retrieve</a:Action>
        <a:To s:mustUnderstand="1">https://mckg4sgmm8lcgdkc-h38b94l2bz0.soap.marketingcloudapis.com/Service.asmx</a:To>
        <fueloauth xmlns="http://exacttarget.com">abc123</fueloauth>
    </s:Header>
    <s:Body xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
        <RetrieveRequestMsg xmlns="http://exacttarget.com/wsdl/partnerAPI">
            <RetrieveRequest>
                <ObjectType>DataFolder</ObjectType>
                <Properties>ObjectID</Properties>
                <Properties>ParentFolder.ID</Properties>
                <Properties>ParentFolder.Name</Properties>
                <Properties>Name</Properties>
                <Properties>Description</Properties>
                <Properties>ContentType</Properties>
                <Properties>IsActive</Properties>
                <Properties>IsEditable</Properties>
                <Filter xsi:type="SimpleFilterPart">
                    <Property>ContentType</Property>
                    <SimpleOperator>equals</SimpleOperator>
                    <Value>dataextension</Value>
                </Filter>
            </RetrieveRequest>
        </RetrieveRequestMsg>
    </s:Body>
</s:Envelope>


Response
<soap:Envelope xmlns:soap="http://www.w3.org/2003/05/soap-envelope" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:wsa="http://schemas.xmlsoap.org/ws/2004/08/addressing" xmlns:wsse="http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd" xmlns:wsu="http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd">
  <env:Header xmlns:env="http://www.w3.org/2003/05/soap-envelope">
    <wsa:Action>RetrieveResponse</wsa:Action>
    <wsa:MessageID>urn:uuid:2ba488cc-f11b-4fd7-a107-b4282f3a963e</wsa:MessageID>
    <wsa:RelatesTo>urn:uuid:0dd284d7-89e5-4c74-a9d5-f84a604d008b</wsa:RelatesTo>
    <wsa:To>http://schemas.xmlsoap.org/ws/2004/08/addressing/role/anonymous</wsa:To>
    <wsse:Security>
      <wsu:Timestamp wsu:Id="Timestamp-d8c5547f-0852-40a8-a350-930a8bd03069">
        <wsu:Created>2025-01-02T16:02:04Z</wsu:Created>
        <wsu:Expires>2025-01-02T16:07:04Z</wsu:Expires>
      </wsu:Timestamp>
    </wsse:Security>
  </env:Header>
  <soap:Body>
    <RetrieveResponseMsg xmlns="http://exacttarget.com/wsdl/partnerAPI">
      <OverallStatus>OK</OverallStatus>
      <RequestID>2c233771-b248-4993-9864-63aafaf109d1</RequestID>
      <Results xsi:type="DataFolder">
        <PartnerKey xsi:nil="true"/>
        <ObjectID>ee813722-f797-4290-a3e1-257950795cf6</ObjectID>
        <ParentFolder>
          <PartnerKey xsi:nil="true"/>
          <ID>0</ID>
          <ObjectID xsi:nil="true"/>
        </ParentFolder>
        <Name>Data Extensions</Name>
        <Description/>
        <ContentType>dataextension</ContentType>
        <IsActive>true</IsActive>
        <IsEditable>false</IsEditable>
      </Results>
      <Results xsi:type="DataFolder">
        <PartnerKey xsi:nil="true"/>
        <ObjectID>1e599348-92c1-42cf-aae0-e163e6923faf</ObjectID>
        <ParentFolder>
          <PartnerKey xsi:nil="true"/>
          <ID>418</ID>
          <ObjectID xsi:nil="true"/>
          <Name>Data Extensions</Name>
        </ParentFolder>
        <Name>Subfolder1</Name>
        <Description/>
        <ContentType>dataextension</ContentType>
        <IsActive>true</IsActive>
        <IsEditable>true</IsEditable>
      </Results>
    </RetrieveResponseMsg>
  </soap:Body>
</soap:Envelope>

    */