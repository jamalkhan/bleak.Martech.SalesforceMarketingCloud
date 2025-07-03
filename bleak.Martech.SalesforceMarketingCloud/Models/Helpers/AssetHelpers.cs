using System.Text.RegularExpressions;
using bleak.Martech.SalesforceMarketingCloud.Models.Pocos;
using bleak.Martech.SalesforceMarketingCloud.Models.SfmcDtos;

namespace bleak.Martech.SalesforceMarketingCloud.Models.Helpers;

public static class AssetHelpers
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

    public static List<ContentBlock> GetContentBlocks(this AssetPoco assets)
    {
        return GetContentBlocks(assets.Content);        
    }
    
    static List<ContentBlock> GetContentBlocks(this string input)
    {
        return GetContentBlocksFromInput(input, ContentBlockType.Key)
            .Concat(GetContentBlocksFromInput(input, ContentBlockType.Name))
            .Concat(GetContentBlocksFromInput(input, ContentBlockType.Id))
            .ToList();
    }
    static List<ContentBlock> GetContentBlocksFromInput(string input, ContentBlockType type)
    {
        if (string.IsNullOrEmpty(input))
        {
            return new List<ContentBlock>();
        }

        string pattern = type switch
        {
            ContentBlockType.Key => @"%%=\s*ContentBlockByKey\s*\(\s*""([^""]+)""\s*\)\s*=%%",
            ContentBlockType.Name => @"%%=\s*ContentBlockByName\s*\(\s*""([^""]+)""\s*\)\s*=%%",
            ContentBlockType.Id => @"%%=\s*ContentBlockByID\s*\(\s*""([^""]+)""\s*\)\s*=%%",
            _ => throw new ArgumentException("Invalid type. Use 'Key' or 'Name'.", nameof(type))
        };

        var matches = Regex.Matches(
            input,
            pattern,
            RegexOptions.IgnoreCase | RegexOptions.Singleline
        );

        return matches
            .Cast<Match>()
            .Select(m => type switch
            {
                ContentBlockType.Key => new ContentBlock() { Key = m.Groups[1].Value },
                ContentBlockType.Name => new ContentBlock() { Name = m.Groups[1].Value },
                ContentBlockType.Id =>
                    int.TryParse(m.Groups[1].Value, out var id)
                    ? new ContentBlock() { Id = id }
                    : new ContentBlock() { Id = null },
                _ => throw new ArgumentException("Invalid type. Use 'Key' or 'Name'.", nameof(type))
            })
            .ToList();
    }

}

public enum ContentBlockType
{
    Key,
    Name,
    Id
}

public class ContentBlock
{
    public int? Id { get; set; }
    public string? Key { get; set; }
    public string? Name { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool Validate()
    {
        var hasId = Id.HasValue;
        var hasKey = !string.IsNullOrEmpty(Key);
        var hasName = !string.IsNullOrEmpty(Name);

        int count = (hasId ? 1 : 0) + (hasKey ? 1 : 0) + (hasName ? 1 : 0);

        if (count == 1)
        {
            return true;
        }
        if (count > 1)
        {
            throw new InvalidOperationException("Only one of Id, Key, or Name can have a value.");
        }
        return false;
    }
}