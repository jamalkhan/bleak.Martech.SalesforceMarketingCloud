using System.Xml.Serialization;
using bleak.Api.Rest;

namespace bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Sfmc.Soap.DataExtensions
{
    public partial class SoapSerializer : ISerializer, IDeserializer
    {
        public T Deserialize<T>(string data) where T : class
        {
            var serializer = new XmlSerializer(typeof(T));
            using (var reader = new StringReader(data))
            {
                var response = (T)serializer.Deserialize(reader);
                return response!;
            }
        }

        public string Serialize(object obj)
        {
            var serializer = new XmlSerializer(obj.GetType());

            using (var stringWriter = new StringWriter())
            {
                serializer.Serialize(stringWriter, obj);
                return stringWriter.ToString();
            }
        }
    }
}