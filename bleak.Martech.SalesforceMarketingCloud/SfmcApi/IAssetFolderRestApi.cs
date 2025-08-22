using bleak.Martech.SalesforceMarketingCloud.Models;

namespace bleak.Martech.SalesforceMarketingCloud.Api;

public interface IAssetFolderRestApi
{
    Task<List<FolderObject>> GetFolderTreeAsync();
}
