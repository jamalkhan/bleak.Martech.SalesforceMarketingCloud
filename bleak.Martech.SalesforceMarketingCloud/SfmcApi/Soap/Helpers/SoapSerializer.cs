using System.Xml.Serialization;
using bleak.Api.Rest;

namespace bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Sfmc.Soap
{
    public partial class SoapSerializer : ISerializer, IDeserializer
    {
        public T Deserialize<T>(string data) where T : class
        {
            var serializer = new XmlSerializer(typeof(T));
            using (var reader = new StringReader(data))
            {
                var deserialized = serializer.Deserialize(reader);
                if (deserialized is T response)
                {
                    return response;
                }
                else
                {
                    throw new InvalidOperationException("Deserialization returned null or incorrect type.");
                }
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