namespace bleak.Martech.SalesforceMarketingCloud
{
    public class SoapApiLog
    {
        public int Thread { get; set; }
        public Guid CorrelationState { get; set; }
        public string Request { get; set; } = string.Empty;
        public string Response { get; set; } = string.Empty;
        public DateTimeOffset RequestTime { get; set; }
        public DateTimeOffset ResponseTime { get; set; }
    }
}