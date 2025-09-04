using System.Xml.Serialization;

namespace bleak.Martech.SalesforceMarketingCloud.Models.Sfmc.Dtos.Soap;

public class SoapBody<T>
    where T : class, new()
{
    [XmlElement("RetrieveResponseMsg", Namespace = "http://exacttarget.com/wsdl/partnerAPI")]
    public RetrieveResponse<T> RetrieveResponse { get; set; } = new();
}
