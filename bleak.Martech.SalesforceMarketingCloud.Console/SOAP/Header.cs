using System.Xml.Serialization;

namespace bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Soap
{
    public class Header
    {
        [XmlElement(ElementName = "Action", Namespace = "http://schemas.xmlsoap.org/ws/2004/08/addressing")]
        public Action Action { get; set; }

        [XmlElement(ElementName = "To", Namespace = "http://schemas.xmlsoap.org/ws/2004/08/addressing")]
        public To To { get; set; }

        [XmlElement(ElementName = "fueloauth", Namespace = "http://exacttarget.com")]
        public string FuelOAuth { get; set; }
    }
}