namespace bleak.Martech.SalesforceMarketingCloud.Models.Pocos
{
    public class ClickEventPoco : BasePoco
    {
        public string SubscriberKey { get; set; } = string.Empty;
        public DateTime EventDate { get; set; } = DateTime.MinValue;
        public string SendID { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
    }
}
