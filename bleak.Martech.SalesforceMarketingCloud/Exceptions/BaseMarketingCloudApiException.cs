namespace bleak.Martech.SalesforceMarketingCloud.Exceptions
{
    public abstract class BaseMarketingCloudApiException : BaseMarketingCloudException
    {
        internal BaseMarketingCloudApiException(string status, string message, string requestId) : base(message)
        {
            Status = status;
            RequestId = requestId;
        }

        public string Status { get; private set; }
        public string RequestId { get; private set; }
    }
}