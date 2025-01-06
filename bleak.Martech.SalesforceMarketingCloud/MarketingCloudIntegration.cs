using bleak.Martech.SalesforceMarketingCloud.Wsdl;

namespace bleak.Martech.SalesforceMarketingCloud
{
    public class MarketingCloudIntegration
    {
        IAuthInfo authInfo;

        internal const int ChunkSize = 50;
        protected const string ErrorStatus = "error";
        protected const string MoreDataAvailableStatus = "moredataavailable";
        protected const string OkStatus = "OK";
        protected const string DefaultSubscriberKeyColumn = "SUBSCRIBER_KEY";


        public MarketingCloudIntegration(IAuthInfo auth)
        {
            authInfo = auth;
        }


        public static void Main()
        {
            //var client = new SoapClient();
            //var describeRequest = new DescribeRequest();
            //var response = client.DescribeAsync(describeRequest);
            //var client = new SfmcServiceReference();
            //var response = await client.YourMethodAsync(yourRequestData);
        }



    }

}