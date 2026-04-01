namespace bleak.Martech.SalesforceMarketingCloud.Configuration
{
    public class SfmcConnectionConfiguration
    {
        public int MaxDegreesOfParallelism { get; set; } = 10;
        public int PageSize { get; set; } = 2500;
        public bool Debug { get; set; } = false;
        public string AuthBaseUrl { get; set; } = string.Empty;
        public string RestBaseUrl { get; set; } = string.Empty;
        public string SoapBaseUrl { get; set; } = string.Empty;

        public SfmcConnectionConfiguration()
        {
        }
        public SfmcConnectionConfiguration(
            int maxDegreesOfParallelism,
            int pageSize,
            bool debug,
            string? authBaseUrl = null,
            string? restBaseUrl = null,
            string? soapBaseUrl = null)
            : this()
        {
            MaxDegreesOfParallelism = maxDegreesOfParallelism;
            PageSize = pageSize;
            Debug = debug;
            AuthBaseUrl = authBaseUrl ?? string.Empty;
            RestBaseUrl = restBaseUrl ?? string.Empty;
            SoapBaseUrl = soapBaseUrl ?? string.Empty;
        }
    }
}
