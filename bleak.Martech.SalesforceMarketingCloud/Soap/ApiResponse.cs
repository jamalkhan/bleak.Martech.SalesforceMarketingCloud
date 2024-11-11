namespace bleak.Martech.SalesforceMarketingCloud
{
    public class ApiResponse<T>
    {
        public T? Response { get; set; }
        public SoapApiLog? Log { get; set; }
    }
}