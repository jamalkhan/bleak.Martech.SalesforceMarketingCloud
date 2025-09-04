using System.Xml.Serialization;

namespace bleak.Martech.SalesforceMarketingCloud.Models.Sfmc.Dtos.Soap;

public class SoapTimestamp
{
    [XmlAttribute("Id", Namespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd")]
    public string Id { get; set; } = "";

    [XmlElement("Created", Namespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd")]
    public DateTime Created { get; set; }

    [XmlElement("Expires", Namespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd")]
    public DateTime Expires { get; set; }
}
