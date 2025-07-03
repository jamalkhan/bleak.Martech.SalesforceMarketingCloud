
namespace bleak.Martech.SalesforceMarketingCloud.Models.SfmcDtos
{
    public partial class DataExtensionDataDto
    {
        public LinksDto links { get; set; } = new LinksDto();
        public string requestToken { get; set; } = string.Empty;
        public DateTime tokenExpireDateUtc { get; set; }
        public string customObjectId { get; set; } = string.Empty;
        public string customObjectKey { get; set; } = string.Empty;
        public int pageSize { get; set; }
        public int page { get; set; }
        public int count { get; set; }
        public int top { get; set; }
        public List<ItemDto> items { get; set; } = new List<ItemDto>();
    }


    public class LinksDto
    {
        public string self { get; set; } = string.Empty;
        public string next { get; set; } = string.Empty;
    }

    public class ItemDto
    {
        public Dictionary<string, string> keys { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> values { get; set; } = new Dictionary<string, string>();
    }

}