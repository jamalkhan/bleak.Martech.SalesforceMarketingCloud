using System;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.Wsdl;

namespace bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Sfmc.Soap
{
    [XmlRoot("Envelope", Namespace = "http://www.w3.org/2003/05/soap-envelope")]
    public class SoapEnvelope<T>
        where T : bleak.Martech.SalesforceMarketingCloud.Wsdl.APIObject
    {
        [XmlElement("Header", Namespace = "http://www.w3.org/2003/05/soap-envelope")]
        public SoapHeader Header { get; set; } = new SoapHeader();

        [XmlElement("Body", Namespace = "http://www.w3.org/2003/05/soap-envelope")]
        public SoapBody<T> Body { get; set; } = new SoapBody<T>();
    }

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

    public class SoapSecurity
    {
        [XmlElement("Timestamp", Namespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd")]
        public SoapTimestamp Timestamp { get; set; } = new SoapTimestamp();
    }

    public class SoapTimestamp
    {
        [XmlAttribute("Id", Namespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd")]
        public string Id { get; set; } = "";

        [XmlElement("Created", Namespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd")]
        public DateTime Created { get; set; }

        [XmlElement("Expires", Namespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd")]
        public DateTime Expires { get; set; }
    }

    public class SoapBody<T>
     where T : bleak.Martech.SalesforceMarketingCloud.Wsdl.APIObject
    {
        [XmlElement("RetrieveResponseMsg", Namespace = "http://exacttarget.com/wsdl/partnerAPI")]
        public RetrieveResponse<T> RetrieveResponse { get; set; } = new RetrieveResponse<T>();
    }

    

    /*
    [XmlRoot("RetrieveResponseMsg", Namespace = "http://exacttarget.com/wsdl/partnerAPI")]
    public class RetrieveResponseMsg
    {
        [XmlElement("OverallStatus")]
        public string OverallStatus { get; set; }

        [XmlElement("RequestID")]
        public string RequestID { get; set; }

        [XmlElement("Results", Namespace = "http://exacttarget.com/wsdl/partnerAPI")]
        public string Results { get; set; } // You can replace this with a more complex type if needed
    }
    */
}