namespace bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Sfmc.Soap
{
    public class SentEventPoco : BasePoco
    {
        public string SubscriberKey { get; set; } = string.Empty;
        public DateTime EventDate { get; set; } = DateTime.MinValue;
        public string SendID { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
    }
}