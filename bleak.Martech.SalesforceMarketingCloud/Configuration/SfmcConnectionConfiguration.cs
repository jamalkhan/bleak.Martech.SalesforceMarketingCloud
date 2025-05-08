namespace bleak.Martech.SalesforceMarketingCloud.Configuration
{
    public class SfmcConnectionConfiguration
    {
        public int MaxDegreesOfParallelism { get; private set; } = 10;
        public int PageSize { get; private set; } = 500;
        public bool Debug { get; private set; } = false;

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