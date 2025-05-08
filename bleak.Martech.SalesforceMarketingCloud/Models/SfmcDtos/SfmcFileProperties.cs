namespace bleak.Martech.SalesforceMarketingCloud.Models.SfmcDtos
{
    public class SfmcFileProperties
        {
            public string fileName {get;set;} = string.Empty;
            public string extension {get;set;} = string.Empty;
            public int fileSize {get;set;}
            public DateTime fileCreatedDate {get;set;}
            public int width {get;set;}
            public int height {get;set;}
            public string publishedURL = string.Empty;
        }
}