using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Configuration;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Authentication;

namespace bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Sfmc.Soap.DataExtensions
{
    public abstract partial class BaseSoapApi
    {
        protected string url = $"https://{AppConfiguration.Instance.Subdomain}.soap.marketingcloudapis.com/Service.asmx";
        protected AuthRepository _authRepository;
        protected RestManager _restManager;

        public BaseSoapApi(AuthRepository authRepository)
        {
            _authRepository = authRepository;
            var soapSerializer = new SoapSerializer();
            _restManager = new RestManager(soapSerializer, soapSerializer);
        }
        
        protected List<Header> BuildHeaders()
        {
            var headers = new List<Api.Rest.Header>();
            headers.Add(new Api.Rest.Header() { Name = "Content-Type", Value = "text/xml" });
            headers.Add(new Api.Rest.Header() { Name = "Accept", Value = "/" });
            headers.Add(new Api.Rest.Header() { Name = "Cache-Control", Value = "no-cache" });
            headers.Add(new Api.Rest.Header() { Name = "Host", Value = $"{AppConfiguration.Instance.Subdomain}.soap.marketingcloudapis.com" });
            return headers;
        }
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