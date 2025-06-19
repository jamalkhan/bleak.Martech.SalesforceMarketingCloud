namespace bleak.Martech.SalesforceMarketingCloud.Models.SfmcDtos
{
    public partial class SfmcAsset
        : ISfmcPoco
    {
        public int id { get; set; }
        public string customerKey { get; set; } = string.Empty;
        public string objectID { get; set; } = string.Empty;
        public SfmcAssetType assetType { get; set; } = new();
        public string name { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public DateTime createdDate { get; set; }
        public SfmcUser createdBy { get; set; } = new();
        public DateTime modifiedDate { get; set; }
        public SfmcUser modifiedBy { get; set; } = new();
        public int enterpriseId { get; set; }
        public int memberId { get; set; }
        public SfmcStatus status { get; set; } = new();
        public SfmcThumbnail thumbnail { get; set; } = new();
        public SfmcCategory category { get; set; } = new();
        public string content { get; set; } = string.Empty;
        public string contentType { get; set; } = string.Empty;
        public SfmcData data { get; set; } = new();
        public SfmcViews views { get; set; } = new();
        public SfmcFileProperties fileProperties { get; set; } = new();

       


    }
}