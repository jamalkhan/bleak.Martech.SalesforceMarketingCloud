using System.Xml.Serialization;

namespace bleak.Martech.SalesforceMarketingCloud.Models.Sfmc.Dtos.Soap;

public class SoapHeader
{
    [XmlElement("Action", Namespace = "http://schemas.xmlsoap.org/ws/2004/08/addressing")]
    public string Action { get; set; } = "";

    [XmlElement("MessageID", Namespace = "http://schemas.xmlsoap.org/ws/2004/08/addressing")]
    public string MessageID { get; set; } = "";

    [XmlElement("RelatesTo", Namespace = "http://schemas.xmlsoap.org/ws/2004/08/addressing")]
    public string RelatesTo { get; set; } = "";

    [XmlElement("To", Namespace = "http://schemas.xmlsoap.org/ws/2004/08/addressing")]
    public string To { get; set; } = "";

    [XmlElement("Security", Namespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd")]
    public SoapSecurity Security { get; set; } = new SoapSecurity();
}
