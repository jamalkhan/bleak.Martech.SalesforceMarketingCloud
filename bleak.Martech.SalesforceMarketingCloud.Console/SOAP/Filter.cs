using System.Xml.Serialization;

namespace bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Soap
{
    public class Filter
    {
        [XmlElement(ElementName = "Property")]
        public string Property { get; set; }

        [XmlElement(ElementName = "SimpleOperator")]
        public string SimpleOperator { get; set; }

        [XmlElement(ElementName = "Value")]
        public string Value { get; set; }

        [XmlAttribute(AttributeName = "xsi:type", Namespace = "http://www.w3.org/2001/XMLSchema-instance")]
        public string XsiType { get; set; }
    }
}