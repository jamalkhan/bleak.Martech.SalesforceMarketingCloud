using System.Xml.Serialization;

namespace bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Soap
{
    public class RetrieveRequest
    {
        [XmlElement(ElementName = "ObjectType")]
        public string ObjectType { get; set; }

        [XmlElement(ElementName = "Properties")]
        public string[] Properties { get; set; }

        [XmlElement(ElementName = "Filter")]
        public Filter Filter { get; set; }
    }
}