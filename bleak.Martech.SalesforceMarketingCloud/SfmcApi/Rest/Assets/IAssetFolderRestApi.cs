using bleak.Martech.SalesforceMarketingCloud.Models;

namespace bleak.Martech.SalesforceMarketingCloud.Sfmc.Rest.Assets
{
    public interface IAssetFolderRestApi
    {
        Task<List<FolderObject>> GetFolderTreeAsync();
    }
}