using System.Xml.Serialization;

namespace bleak.Martech.SalesforceMarketingCloud.Models.Sfmc.Dtos.Soap;

public class SoapSecurity
{
    [XmlElement("Timestamp", Namespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd")]
    public SoapTimestamp Timestamp { get; set; } = new SoapTimestamp();
}
