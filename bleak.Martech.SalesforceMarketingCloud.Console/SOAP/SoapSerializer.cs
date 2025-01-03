using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using bleak.Api.Rest;

namespace bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Soap
{
    public class SoapSerializer : ISerializer, IDeserializer
    {
        /// <summary>
        /// Serializes an Object
        /// </summary>
        /// <param name="obj">The Object to be serialized</param>
        /// <returns>A JSON string of the Object</returns>
        public string Serialize(object obj)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Deserializes a JSON string
        /// </summary>
        /// <typeparam name="T">The Type to cast the JSON String to.</typeparam>
        /// <param name="json">The JSON string to Deserialize</param>
        /// <returns>An Instantiated Object from the JSON</returns>
        public T Deserialize<T>(string soapXml)
            where T : class
        {
            var serializer = new XmlSerializer(typeof(SoapEnvelope<DataFolder>));
            using (var stringReader = new StringReader(soapXml))
            {
                var envelope = (T)serializer.Deserialize(stringReader);

                return envelope;
            }
        }
    }

    [XmlRoot(ElementName = "Envelope", Namespace = "http://www.w3.org/2003/05/soap-envelope")]
    public class SoapEnvelope<T>
    where T : class
    {
        [XmlElement(ElementName = "Body", Namespace = "http://www.w3.org/2003/05/soap-envelope")]
        public SoapBody<T> Body { get; set; }
    }

    public class SoapBody<T>
    where T : class
    {
        [XmlElement(ElementName = "RetrieveResponseMsg", Namespace = "http://exacttarget.com/wsdl/partnerAPI")]
        public RetrieveResponseMsg<T> Response { get; set; }
    }

    [XmlRoot(ElementName = "RetrieveResponseMsg", Namespace = "http://exacttarget.com/wsdl/partnerAPI")]
    public class RetrieveResponseMsg<T>
    where T : class
    {
        public string OverallStatus { get; set; }
        public string RequestID { get; set; }

        [XmlElement(ElementName = "Results")]
        public List<T> Results { get; set; }
    }

    public class DataFolder
    {
        public string ObjectID { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public bool IsEditable { get; set; }
    }
}