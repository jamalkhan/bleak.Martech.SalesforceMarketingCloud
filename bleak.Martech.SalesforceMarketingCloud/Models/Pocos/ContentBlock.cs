using System.Text.RegularExpressions;
using bleak.Martech.SalesforceMarketingCloud.Models.Helpers;

namespace bleak.Martech.SalesforceMarketingCloud.Models.Pocos;

public class ContentBlock
{
    public int? Id { get; set; }
    public string? Key { get; set; }
    public string? Name { get; set; }
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

    public string ContentRegex
    {
        get
        {
            if (!string.IsNullOrEmpty(RegexContentBlockByName))
                return RegexContentBlockByName;
            if (!string.IsNullOrEmpty(RegexContentBlockById))
                return RegexContentBlockById;
            if (!string.IsNullOrEmpty(RegexContentBlockByKey))
                return RegexContentBlockByKey;
            return string.Empty;
        }
    }

    private string? RegexContentBlockByName
    {
        get
        {
            if (Name == null)
            {
                return null;
            }

            string escapedName = Regex.Escape(Name);
            return $"{AssetHelpers.RegexNameStart}{escapedName}{AssetHelpers.RegexNameEnd}";
        }
    }
    private string? RegexContentBlockByKey
    {
        get
        {
            if (Key == null)
            {
                return null;
            }

            string escapedKey = Regex.Escape(Key);
            return $"{AssetHelpers.RegexKeyStart}{escapedKey}{AssetHelpers.RegexKeyEnd}";
        }
    }
    private string? RegexContentBlockById
    {
        get
        {
            if (Id == null)
            {
                return null;
            }

            return $"{AssetHelpers.RegexIdStart}{Id}{AssetHelpers.RegexIdEnd}";
        }
    }
}