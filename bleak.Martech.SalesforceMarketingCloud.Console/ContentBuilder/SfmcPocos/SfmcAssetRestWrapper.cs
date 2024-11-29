namespace bleak.Martech.SalesforceMarketingCloud.ContentBuilder.SfmcPocos
{
    public class SfmcAssetRestWrapper
    {
        public int count { get; set; }
        public int page { get; set; }
        public int pageSize { get; set; }
        public Dictionary<string, object> links { get; set; } = new();
        public List<SfmcAsset> items { get; set; } = new();
    }


}