using System.Text.RegularExpressions;
using bleak.Martech.SalesforceMarketingCloud.Models.Pocos;
using Microsoft.Extensions.Logging;
using SfmcApp.Models.ViewModels;

namespace SfmcApp.ViewModels;

public partial class SfmcAssetListViewModel
{
    #region TODO: Move out... content block expansion logic
    private async Task<string> GetExpandedContentAsync(AssetViewModel asset)
    {
        _logger.LogInformation("Expanding content for asset: {AssetName}", asset.Name);
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
                // TODO: logger
                Console.WriteLine("Breaking out of FillContentExpandedAsync loop after 20 iterations.");
                // Will not do more than 20 levels of recursion.
                break; // Prevent infinite loop
            }

            var subContentBlocks = GetContentBlocksByString(content);
            Console.WriteLine($"Found {subContentBlocks.Count} content blocks in contentExpanded on iteration {i}.");
            if (subContentBlocks == null || subContentBlocks.Count == 0)
            {
                // TODO: logger
                Console.WriteLine("No sub content blocks found in contentExpanded. Breaking out of loop.");
                break;
            }


            foreach (var subContentBlock in subContentBlocks)
            {
                string pattern = subContentBlock.ContentRegex;

                content = await PerformRegexReplacementAsync
                (
                    subContentBlock: subContentBlock,
                    input: content
                );
            }
        }

        _logger.LogInformation("Expanded content for asset: {AssetName} completed.", asset.Name);
        return content;
    }

    List<ContentBlock> GetContentBlocksByString(string input)
    {
        var retval = GetContentBlocksFromInputAsync(input, ContentBlockType.Key)
            .Concat(GetContentBlocksFromInputAsync(input, ContentBlockType.Name))
            .Concat(GetContentBlocksFromInputAsync(input, ContentBlockType.Id))
            .ToList();
        _logger.LogInformation("Found {Count} content blocks in input.", retval.Count);
        return retval;
    }
    const string RegexKeyStart = @"%%=\s*ContentBlockByKey\s*\(\s*""";
    const string RegexKeyEnd = @"""\s*(?:,.*?)?\)\s*=%%";
    const string RegexKeyCapture = "([^\\\"]+)";
    const string RegexNameStart = @"%%=\s*ContentBlockByName\s*\(\s*""";
    const string RegexNameEnd = @"""\s*(?:,.*?)?\)\s*=%%";
    const string RegexNameCapture = @"([^""]+)";

    const string RegexIdStart = @"%%=\s*ContentBlockByID\s*\(\s*[""']?";
    const string RegexIdEnd = @"[""']?\s*(?:,.*?)?\)\s*=%%";
    const string RegexIdCapture = @"(\d+)";
    static List<ContentBlock> GetContentBlocksFromInputAsync(string input, ContentBlockType type)
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


    public async Task<string> PerformRegexReplacementAsync(
            bleak.Martech.SalesforceMarketingCloud.Models.Pocos.ContentBlock subContentBlock,
            string input
            )
    {
        _logger.LogInformation("Performing regex replacement for ContentBlock: {ContentBlock}", subContentBlock);
        bleak.Martech.SalesforceMarketingCloud.Models.Pocos.AssetPoco? subAsset = null;
        if (subContentBlock.Id != null)
        {
            _logger.LogInformation($"Performing regex replacement for Id: {subContentBlock.Id.Value}");
            subAsset = await ContentResourceApi.GetAssetAsync(assetId: subContentBlock.Id.Value);
        }
        else if (!string.IsNullOrEmpty(subContentBlock.Key))
        {
            _logger.LogInformation($"Performing regex replacement for Key: {subContentBlock.Key}");
            subAsset = await ContentResourceApi.GetAssetAsync(customerKey: subContentBlock.Key);
        }
        else if (!string.IsNullOrEmpty(subContentBlock.Name))
        {
            _logger.LogInformation($"Performing regex replacement for Name: {subContentBlock.Name}");
            subAsset = await ContentResourceApi.GetAssetAsync(name: subContentBlock.Name);
        }
        if (subAsset == null)
        {
            _logger.LogWarning($"Sub asset not found for ContentBlock: {subContentBlock}");
            return input; // No replacement if sub asset is not found
        }
        _logger.LogInformation($"Sub asset found: {subAsset.Name} (ID: {subAsset.Id})");

        string subContent = string.Empty;
        if (!string.IsNullOrEmpty(subAsset.Content))
        {
            subContent = subAsset.Content;
            _logger.LogInformation($"Using Content from sub asset: {subAsset.Name}. Content: {subAsset.Content}");
        }
        else if (subAsset != null && subAsset.Views != null && subAsset.Views.Html != null && !string.IsNullOrEmpty(subAsset.Views.Html.Content))
        {
            subContent = subAsset.Views.Html.Content;
            _logger.LogInformation($"Using Views.Html.Content from sub asset: {subAsset.Name}. Views.Html.Content: {subAsset.Views.Html.Content}");
        }

        _logger.LogInformation("----------********************----------");
        _logger.LogInformation($"input: {input}");
        _logger.LogInformation($"pattern: {subContentBlock.ContentRegex}");
        _logger.LogInformation($"replacement: {subContent}");
        _logger.LogInformation("----------********************----------");

        var results = Regex.Replace
                (
                    input: input,
                    pattern: subContentBlock.ContentRegex,
                    replacement: subContent,
                    options: RegexOptions.IgnoreCase | RegexOptions.Singleline
                );
        _logger.LogInformation($"Replaced content for ContentBlock: {subContentBlock}. Result: {results}");
        return results;

    }
#endregion TODO: Move out... content block expansion logic
}
