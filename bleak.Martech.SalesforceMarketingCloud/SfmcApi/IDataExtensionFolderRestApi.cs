using bleak.Martech.SalesforceMarketingCloud.Models;

namespace bleak.Martech.SalesforceMarketingCloud.Api;

public interface IDataExtensionFolderRestApi
{
    Task<List<FolderObject>> GetFolderTreeAsync();
}
