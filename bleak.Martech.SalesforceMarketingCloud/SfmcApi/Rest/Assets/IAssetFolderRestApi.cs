using bleak.Martech.SalesforceMarketingCloud.Models;

namespace bleak.Martech.SalesforceMarketingCloud.Sfmc.Rest.Assets
{
    public interface IAssetFolderRestApi
    {
        List<FolderObject> GetFolderTree();
        Task<List<FolderObject>> GetFolderTreeAsync();
    }
}