using System;
using System.Xml.Serialization;

namespace bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Soap
{
    [XmlRoot(ElementName = "Envelope", Namespace = "http://www.w3.org/2003/05/soap-envelope")]
    public class Envelope
    {
        [XmlElement(ElementName = "Header", Namespace = "http://www.w3.org/2003/05/soap-envelope")]
        public Header Header { get; set; }

        [XmlElement(ElementName = "Body", Namespace = "http://www.w3.org/2003/05/soap-envelope")]
        public Body Body { get; set; }

        [XmlAttribute(AttributeName = "xmlns:s")]
        public string XmlnsS { get; set; }

        [XmlAttribute(AttributeName = "xmlns:a")]
        public string XmlnsA { get; set; }

        [XmlAttribute(AttributeName = "xmlns:u")]
        public string XmlnsU { get; set; }
    }
}