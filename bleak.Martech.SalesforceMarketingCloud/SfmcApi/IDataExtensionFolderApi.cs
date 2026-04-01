using bleak.Martech.SalesforceMarketingCloud.Models.Pocos;

namespace bleak.Martech.SalesforceMarketingCloud.Api;

public interface IDataExtensionFolderApi
{
    Task<List<DataExtensionFolder>> GetFolderTreeAsync();
}
