using bleak.Martech.SalesforceMarketingCloud.Models;

namespace bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Sfmc.Rest.Assets
{
    public interface IAssetFolderRestApi
    {
        List<FolderObject> GetFolderTree();
        Task<List<FolderObject>> GetFolderTreeAsync();
    }
}