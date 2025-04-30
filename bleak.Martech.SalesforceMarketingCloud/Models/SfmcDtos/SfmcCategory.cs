namespace bleak.Martech.SalesforceMarketingCloud.Models.SfmcDtos
{
    public class SfmcCategory
        {
            public int id { get; set; }
            public int parentId { get; set; }
            public string name { get; set; } = string.Empty;
        }

}