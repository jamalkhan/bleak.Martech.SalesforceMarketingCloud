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




    public partial class SfmcAsset
    {
        
        public AssetObject ToAssetObject()
        {
            var retval = new AssetObject()
            {
                Id = id,
                CustomerKey = customerKey,
                ObjectID = objectID,
                AssetType = new AssetObject.AssetTypeObject()
                {
                    Id = assetType.id,
                    Name = assetType.name,
                    DisplayName = assetType.displayName 
                },
                Name = name,
                Description = description,
                CreatedDate = createdDate,
                // TODO
                // UserObject = null
                ModifiedDate = modifiedDate,
                // TODO:
                //ModifiedBy = modifiedBy,
                EnterpriseId= enterpriseId,
                MemberId = memberId,
                // TODO:
                //Status { get; set; } = new();
                // TODO:
                //Thumbnail { get; set; } = new();
                // TODO:
                //Category = new CategoryObject() { // TODO: }
                Content = content,
                //Data = new DataObject

            };
            if (views != null)
            {
                retval.Views = new AssetObject.ViewsObject();
                if (views.html != null)
                {
                    retval.Views.Html = new AssetObject.HtmlObject();
                    retval.Views.Html.Content = views.html.content;
                }
            }
            if (fileProperties != null)
            {
                retval.FileProperties = new AssetObject.FilePropertiesObject();
                retval.FileProperties.FileName = fileProperties.fileName;
                retval.FileProperties.Extension = fileProperties.extension;
                retval.FileProperties.FileSize = fileProperties.fileSize;
                retval.FileProperties.FileCreatedDate = fileProperties.fileCreatedDate;
                retval.FileProperties.Width = fileProperties.width;
                retval.FileProperties.Height = fileProperties.height;
                retval.FileProperties.PublishedURL = fileProperties.publishedURL;
            }

            return retval;
        }
    }
}