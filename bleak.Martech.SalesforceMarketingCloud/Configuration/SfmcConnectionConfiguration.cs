namespace bleak.Martech.SalesforceMarketingCloud.Configuration
{
    public class SfmcConnectionConfiguration
    {
        public int MaxDegreesOfParallelism { get; set; } = 10;
        public int PageSize { get; set; } = 2500;
        public bool Debug { get; set; } = false;

        public SfmcConnectionConfiguration()
        {
        }
        public SfmcConnectionConfiguration(int maxDegreesOfParallelism, int pageSize, bool debug)
            : this()
        {
            MaxDegreesOfParallelism = maxDegreesOfParallelism;
            PageSize = pageSize;
            Debug = debug;
        }
    }
}