
namespace bleak.Martech.SalesforceMarketingCloud.ContentBuilder
{
    public class SfmcRestWrapper
    {
        public int count { get; set; }

        public int page { get; set; }

        public int pageSize { get; set; }

        public Dictionary<string, object> links { get; set; } = new();

        public List<SfmcFolder> items { get; set; } = new();
    }

    public class SfmcFolder
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