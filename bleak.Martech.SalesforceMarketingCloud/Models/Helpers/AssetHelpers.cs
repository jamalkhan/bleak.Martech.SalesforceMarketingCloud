using System.Data;
using System.Text.RegularExpressions;
using bleak.Martech.SalesforceMarketingCloud.Models.Pocos;
using bleak.Martech.SalesforceMarketingCloud.Models.Sfmc;
using bleak.Martech.SalesforceMarketingCloud.Sfmc.Rest.Assets;
using bleak.Martech.SalesforceMarketingCloud.Api;
using Microsoft.Extensions.Logging;

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

    /// <summary>
    /// Expands the <c>ContentExpanded</c> property of the given <see cref="AssetPoco"/> instance by recursively replacing content blocks
    /// found within the asset's content using the provided <see cref="IAssetRestApi"/>.
    /// The method will perform up to 20 iterations to prevent infinite recursion.
    /// </summary>
    /// <param name="asset">The <see cref="AssetPoco"/> instance whose content is to be expanded.</param>
    /// <param name="api">The <see cref="IAssetRestApi"/> used to retrieve and replace content blocks.</param>
    /// <remarks>
    /// This method initializes <c>ContentExpanded</c> with the value of <c>Content</c> and iteratively replaces content blocks
    /// found within it. The process stops if no more content blocks are found or after 20 iterations.
    /// </remarks>
    public static async Task<string> GetExpandedContentAsync(this AssetPoco asset, IAssetRestApi api, ILogger? logger = null)
    {
        string content = string.Empty;
        if (!string.IsNullOrEmpty(asset.Content))
        {
            content = asset.Content;
        }
        else if (asset.Views?.Html?.Content != null)
        {
            content = asset.Views.Html.Content;
        }
        
        int i = 0;
        while (true)
        {
            i++;
            if (i > 20 || string.IsNullOrEmpty(content))
            {
                logger?.LogDebug("Stopping expanded content resolution. Iteration={Iteration}, HasContent={HasContent}", i, !string.IsNullOrEmpty(content));
                // Will not do more than 20 levels of recursion.
                break; // Prevent infinite loop
            }

            var subContentBlocks = GetContentBlocksByString(content);
            logger?.LogTrace("Expanded content iteration {Iteration} found {ContentBlockCount} content blocks.", i, subContentBlocks.Count);
            if (subContentBlocks == null || subContentBlocks.Count == 0)
            {
                logger?.LogTrace("No additional content blocks found during content expansion at iteration {Iteration}.", i);
                break;
            }


            foreach (var subContentBlock in subContentBlocks)
            {
                string pattern = subContentBlock.ContentRegex;

                content = await PerformRegexReplacementAsync
                (
                    api: api,
                    subContentBlock: subContentBlock,
                    input: content,
                    logger: logger
                );
            }
        }

        return content;
    }



    /// <summary>
    /// Performs a regex replacement on the input string using the content of the specified sub-content block.
    /// The sub-content block is retrieved using the provided <see cref="IAssetRestApi"/> based on its ID, customer key, or name.
    /// The method uses the content regex defined in the sub-content block to find matches in the input string and replace them with the content of the sub-content block.
    /// If the sub-content block is not found, the input string is returned unchanged.
    /// </summary>
    /// <param name="api">The <see cref="IAssetRestApi"/> used to retrieve and replace content blocks.</param>
    /// <param name="subContentBlock">The sub-content block to use for the replacement.</param>
    /// <param name="input">The input string to perform the replacement on.</param>
    /// <returns>The modified input string with the replacement applied, or the original input if no replacement was made.</returns>
    public static async Task<string> PerformRegexReplacementAsync(
        IAssetRestApi api,
        ContentBlock subContentBlock,
        string input,
        ILogger? logger = null
        )
    {

        AssetPoco? subAsset = null;
        if (subContentBlock.Id != null)
        {
            logger?.LogTrace("Performing regex replacement using content block Id {AssetId}.", subContentBlock.Id.Value);
            subAsset = await api.GetAssetAsync(assetId: subContentBlock.Id.Value);
        }
        else if (!string.IsNullOrEmpty(subContentBlock.Key))
        {
            logger?.LogTrace("Performing regex replacement using content block key {CustomerKey}.", subContentBlock.Key);
            subAsset = await api.GetAssetAsync(customerKey: subContentBlock.Key);
        }
        else if (!string.IsNullOrEmpty(subContentBlock.Name))
        {
            logger?.LogTrace("Performing regex replacement using content block name {Name}.", subContentBlock.Name);
            subAsset = await api.GetAssetAsync(name: subContentBlock.Name);
        }
        if (subAsset == null)
        {
            logger?.LogWarning("Sub asset not found for content block {@ContentBlock}.", subContentBlock);
            return input; // No replacement if sub asset is not found
        }

        string subContent = string.Empty;
        if (!string.IsNullOrEmpty(subAsset.Content))
        {
            subContent = subAsset.Content;
            logger?.LogTrace("Using direct content from sub asset {AssetName} (Id={AssetId}).", subAsset.Name, subAsset.Id);
        }
        else if (subAsset.Views?.Html?.Content != null)
        {
            subContent = subAsset.Views.Html.Content;
            logger?.LogTrace("Using HTML view content from sub asset {AssetName} (Id={AssetId}).", subAsset.Name, subAsset.Id);
        }
        logger?.LogTrace("Applying regex replacement. Pattern={Pattern}, InputLength={InputLength}, ReplacementLength={ReplacementLength}", subContentBlock.ContentRegex, input.Length, subContent.Length);
        
        var results = Regex.Replace
                (
                    input: input,
                    pattern: subContentBlock.ContentRegex,
                    replacement: subContent,
                    options: RegexOptions.IgnoreCase | RegexOptions.Singleline
                );
        logger?.LogTrace("Regex replacement complete for content block {@ContentBlock}. ResultLength={ResultLength}", subContentBlock, results.Length);
        return results;

    }

    public static List<ContentBlock> GetContentBlocks(this AssetPoco assets)
    {
        return GetContentBlocksByString(assets.Content);
    }


    static List<ContentBlock> GetContentBlocksByString(this string input)
    {
        return GetContentBlocksFromInput(input, ContentBlockType.Key)
            .Concat(GetContentBlocksFromInput(input, ContentBlockType.Name))
            .Concat(GetContentBlocksFromInput(input, ContentBlockType.Id))
            .ToList();
    }
    

    public const string RegexKeyStart = @"%%=\s*ContentBlockByKey\s*\(\s*""";
    public const string RegexKeyEnd = @"""\s*(?:,.*?)?\)\s*=%%";
    public const string RegexKeyCapture = "([^\\\"]+)";
    public const string RegexNameStart = @"%%=\s*ContentBlockByName\s*\(\s*""";
    public const string RegexNameEnd = @"""\s*(?:,.*?)?\)\s*=%%";
    public const string RegexNameCapture = @"([^""]+)";

    public const string RegexIdStart = @"%%=\s*ContentBlockByID\s*\(\s*[""']?";
    public const string RegexIdEnd = @"[""']?\s*(?:,.*?)?\)\s*=%%";
    public const string RegexIdCapture = @"(\d+)";
    static List<ContentBlock> GetContentBlocksFromInput(string input, ContentBlockType type)
    {
        if (string.IsNullOrEmpty(input))
        {
            return new List<ContentBlock>();
        }

        string pattern = type switch
        {
            ContentBlockType.Key => $"{RegexKeyStart}{RegexKeyCapture}{RegexKeyEnd}",
            ContentBlockType.Name => $"{RegexNameStart}{RegexNameCapture}{RegexNameEnd}",
            ContentBlockType.Id => $"{RegexIdStart}{RegexIdCapture}{RegexIdEnd}",
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
