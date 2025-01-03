using System.Xml.Serialization;

namespace bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Soap
{
    public class RetrieveRequestMsg
    {
        [XmlElement(ElementName = "RetrieveRequest")]
        public RetrieveRequest RetrieveRequest { get; set; }

        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }
    }
}