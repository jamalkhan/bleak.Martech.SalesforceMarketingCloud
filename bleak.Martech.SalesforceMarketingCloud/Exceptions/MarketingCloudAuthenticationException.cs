namespace bleak.Martech.SalesforceMarketingCloud.Exceptions
{
    public class MarketingCloudAuthenticationException : BaseMarketingCloudException
    {
        internal MarketingCloudAuthenticationException() : base("Login Failed") { }
    }
}