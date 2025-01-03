using System.Xml.Serialization;

namespace bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Soap
{
    public class Body
    {
        [XmlElement(ElementName = "RetrieveRequestMsg", Namespace = "http://exacttarget.com/wsdl/partnerAPI")]
        public RetrieveRequestMsg RetrieveRequestMsg { get; set; }
    }
}