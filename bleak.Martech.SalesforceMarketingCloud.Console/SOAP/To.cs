using System.Xml.Serialization;

namespace bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Soap
{
    public class To
    {
        [XmlText]
        public string Value { get; set; }

        [XmlAttribute(AttributeName = "s:mustUnderstand")]
        public int MustUnderstand { get; set; }
    }
}