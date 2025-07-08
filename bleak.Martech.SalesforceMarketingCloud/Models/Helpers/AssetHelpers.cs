using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using bleak.Martech.SalesforceMarketingCloud.Models.Pocos;
using bleak.Martech.SalesforceMarketingCloud.Models.SfmcDtos;
using bleak.Martech.SalesforceMarketingCloud.Sfmc.Rest.Assets;

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

    public async static void FillContentExpandedAsync(this AssetPoco asset, IAssetRestApi api)
    {
        int i = 0;
        while (true)
        {
            i++;
            if (i > 20 || string.IsNullOrEmpty(asset.ContentExpanded))
            {
                break; // Prevent infinite loop
            }

            var contentBlocks = GetContentBlocks(asset);
            if (contentBlocks == null || contentBlocks.Count == 0)
            {
                break;
            }
            foreach (var contentBlock in contentBlocks)
            {
                asset.ContentExpanded =
                    Regex.Replace(
                        asset.ContentExpanded ?? asset.Content,
                        contentBlock.ContentRegex,
                        match =>
                            {
                                var subAsset =
                                    api.GetAsset
                                    (
                                        assetId: contentBlock.Id,
                                        customerKey: contentBlock.Key,
                                        name: contentBlock.Key
                                    );
                                return subAsset.Content;
                            },
                        RegexOptions.IgnoreCase | RegexOptions.Singleline
                    );
            }
        }

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
            //ContentBlockType.Key => @"%%=\s*ContentBlockByKey\s*\(\s*""([^""]+)""\s*\)\s*=%%",
            ContentBlockType.Key => @"%%=\s*ContentBlockByKey\s*\(\s*""([^""]+)""(.*?)\)\s*=%%",
            //ContentBlockType.Name => @"%%=\s*ContentBlockByName\s*\(\s*""([^""]+)""\s*\)\s*=%%",
            ContentBlockType.Name => @"%%=\s*ContentBlockByName\s*\(\s*""([^""]+)""(.*?)\)\s*=%%",
            ContentBlockType.Id => @"%%=\s*ContentBlockByID\s*\(\s*[""']?(\d+)[""']?(?:\s*,[^)]*)?\s*\)\s*=%%",
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
