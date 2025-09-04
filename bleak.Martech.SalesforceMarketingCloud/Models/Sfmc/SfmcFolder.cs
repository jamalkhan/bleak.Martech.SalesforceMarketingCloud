namespace bleak.Martech.SalesforceMarketingCloud.Models.SfmcDtos
{
    public partial class SfmcFolder : ISfmcPoco
    {
        public int id { get; set; }

        public string description { get; set; } = string.Empty;

        public int enterpriseId { get; set; }

        public int memberId { get; set; }

        public string name { get; set; } = string.Empty;

        public int parentId { get; set; }

        public string categoryType { get; set; } = string.Empty;
    }
}