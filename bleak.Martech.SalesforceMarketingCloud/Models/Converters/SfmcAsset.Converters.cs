using bleak.Martech.SalesforceMarketingCloud.Models.Pocos;

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

    public partial class DataExtensionDataDto
    {
        public List<Dictionary<string, string>> ToDictionaryList()
        {
            var retval = new List<Dictionary<string,string>>();
            foreach (var item in items)
            {
                var dict = new Dictionary<string,string>();
                foreach (var key in item.keys)
                {
                    dict[key.Key] = key.Value;
                }
                foreach (var value in item.values)
                {
                    dict[value.Key] = value.Value;
                }
                retval.Add(dict);
            }
            return retval;
        }
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


    public static class SfmcAssetConverters
    {
        public static List<AssetPoco> ToPocoList(this IEnumerable<SfmcAsset> assets)
        {
            return assets.Select(asset => asset.ToPoco()).ToList();
        }
        public static AssetPoco ToPoco(this SfmcAsset asset)
        {
            var retval = new AssetPoco()
            {
                Id = asset.id,
                CustomerKey = asset.customerKey,
                ObjectID = asset.objectID,
                AssetType = new AssetPoco.AssetTypeObject()
                {
                    Id = asset.assetType.id,
                    Name = asset.assetType.name,
                    DisplayName = asset.assetType.displayName
                },
                Name = asset.name,
                Description = asset.description,
                CreatedDate = asset.createdDate,
                // TODO
                // UserObject = null
                ModifiedDate = asset.modifiedDate,
                // TODO:
                //ModifiedBy = modifiedBy,
                EnterpriseId = asset.enterpriseId,
                MemberId = asset.memberId,
                // TODO:
                //Status { get; set; } = new();
                // TODO:
                //Thumbnail { get; set; } = new();
                // TODO:
                //Category = new CategoryObject() { // TODO: }
                Content = asset.content,
                ContentType = asset.contentType,
                //Data = new DataObject
            };
            if (asset.views != null)
            {
                retval.Views = new AssetPoco.ViewsObject();
                if (asset.views.html != null)
                {
                    retval.Views.Html = new AssetPoco.HtmlObject();
                    retval.Views.Html.Content = asset.views.html.content;
                }
            }
            if (asset.fileProperties != null)
            {
                retval.FileProperties = new AssetPoco.FilePropertiesObject();
                retval.FileProperties.FileName = asset.fileProperties.fileName;
                retval.FileProperties.Extension = asset.fileProperties.extension;
                retval.FileProperties.FileSize = asset.fileProperties.fileSize;
                retval.FileProperties.FileCreatedDate = asset.fileProperties.fileCreatedDate;
                retval.FileProperties.Width = asset.fileProperties.width;
                retval.FileProperties.Height = asset.fileProperties.height;
                retval.FileProperties.PublishedURL = asset.fileProperties.publishedURL;
            }

            return retval;
        }
    }
}