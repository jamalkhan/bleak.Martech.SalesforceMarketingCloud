using bleak.Martech.SalesforceMarketingCloud.Wsdl;

namespace bleak.Martech.SalesforceMarketingCloud.Exceptions
{
    public class MarketingCloudStatusException : BaseMarketingCloudApiException
    {
        internal MarketingCloudStatusException(GetSystemStatusResponse response) : base(status: response.OverallStatus, message: response.OverallStatusMessage, requestId: response.RequestID) { }
    }
}