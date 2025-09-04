using System.Xml.Serialization;

namespace bleak.Martech.SalesforceMarketingCloud.Models.Sfmc.Dtos.Soap;

public class SoapEnvelope<T>
    where T : class, new()
{
    [XmlElement("Header", Namespace = "http://www.w3.org/2003/05/soap-envelope")]
    public SoapHeader Header { get; set; } = new SoapHeader();

    [XmlElement("Body", Namespace = "http://www.w3.org/2003/05/soap-envelope")]
    public SoapBody<T> Body { get; set; } = new SoapBody<T>();
}
