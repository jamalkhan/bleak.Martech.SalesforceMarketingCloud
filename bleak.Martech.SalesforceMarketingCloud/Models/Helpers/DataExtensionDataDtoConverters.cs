using bleak.Martech.SalesforceMarketingCloud.Models.SfmcDtos;

namespace bleak.Martech.SalesforceMarketingCloud.Models.Helpers;

public static class DataExtensionDataDtoHelpers
{
    public static List<Dictionary<string, string>> ToDictionaryList(this DataExtensionDataDto dto)
    {
        var retval = new List<Dictionary<string,string>>();
        foreach (var item in dto.items)
        {
            var dict = new Dictionary<string,string>();
            if (item.keys != null)
            {
                foreach (var key in item.keys)
                {
                    dict[key.Key] = key.Value;
                }
            }
            if (item.values != null)
            {
                foreach (var value in item.values)
                {
                    dict[value.Key] = value.Value;
                }
            }
            retval.Add(dict);
        }
        return retval;
    }
}
