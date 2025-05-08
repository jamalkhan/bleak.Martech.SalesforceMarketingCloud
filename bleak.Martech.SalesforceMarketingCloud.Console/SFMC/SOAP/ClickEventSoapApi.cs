using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Configuration;
using bleak.Martech.SalesforceMarketingCloud.Authentication;
using bleak.Martech.SalesforceMarketingCloud.Wsdl;
using System.Text;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Fileops;
using bleak.Martech.SalesforceMarketingCloud.Configuration;

namespace bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Sfmc.Soap
{
    public partial class ClickEventSoapApi
        : BaseDataSetSoapApi<ClickEvent, ClickEventPoco>
    {
        public DateTime StartDate { get; set; }  = DateTime.MinValue;
        public DateTime EndDate { get; set; } = DateTime.MinValue;

        public ClickEventSoapApi(
            IAuthRepository authRepository, 
            IFileWriter fileWriter, 
            DateTime startDate, 
            DateTime endDate)
            : base(
                authRepository: authRepository, 
                fileWriter: fileWriter, 
                config: new SfmcConnectionConfiguration())
        {
            StartDate = startDate;
            EndDate = endDate;
        }

        public override ClickEventPoco ConvertToPoco(ClickEvent wsdlObject)
        {
            var poco = new ClickEventPoco();
            poco.SubscriberKey = wsdlObject.SubscriberKey;
            poco.EventDate = wsdlObject.EventDate;
            poco.SendID = wsdlObject.SendID.ToString();
            poco.EventType = wsdlObject.EventType.ToString();
            return poco;
        }

        public override string BuildRequest()
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
            if (!string.IsNullOrEmpty(RequestID))
            {
                sb.AppendLine($"                <ContinueRequest>{RequestID}</ContinueRequest>");
            }
            sb.AppendLine($"                <ObjectType>ClickEvent</ObjectType>");
            sb.AppendLine($"                <Properties>SubscriberKey</Properties>");
            sb.AppendLine($"                <Properties>EventDate</Properties>");
            sb.AppendLine($"                <Properties>SendID</Properties>");
            sb.AppendLine($"                <Properties>EventType</Properties>");
            if (string.IsNullOrEmpty(RequestID))
            {
                sb.AppendLine($"                <Filter xsi:type=\"ComplexFilterPart\">");
                sb.AppendLine($"                    <LeftOperand xsi:type=\"SimpleFilterPart\">");
                sb.AppendLine($"                            <Property>EventDate</Property>");
                sb.AppendLine($"                            <SimpleOperator>greaterThanOrEqual</SimpleOperator>");
                sb.AppendLine($"                            <Value>{StartDate.ToString("yyyy-MM-ddTHH:mm:ssZ")}</Value>");
                sb.AppendLine($"                    </LeftOperand>");
                sb.AppendLine($"                    <LogicalOperator>AND</LogicalOperator>");
                sb.AppendLine($"                    <RightOperand xsi:type=\"SimpleFilterPart\">");
                sb.AppendLine($"                            <Property>EventDate</Property>");
                sb.AppendLine($"                            <SimpleOperator>lessThan</SimpleOperator>");
                sb.AppendLine($"                            <Value>{EndDate.ToString("yyyy-MM-ddTHH:mm:ssZ")}</Value>");
                sb.AppendLine($"                    </RightOperand>");
                sb.AppendLine($"                </Filter>");
            }
            sb.AppendLine($"             </RetrieveRequest>");
            sb.AppendLine($"        </RetrieveRequestMsg>");
            sb.AppendLine($"    </s:Body>");
            sb.AppendLine($"</s:Envelope>");
            return sb.ToString();
        }
    }
}