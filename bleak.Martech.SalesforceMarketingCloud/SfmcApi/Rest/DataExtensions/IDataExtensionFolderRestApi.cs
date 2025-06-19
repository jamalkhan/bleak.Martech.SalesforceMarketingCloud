using bleak.Martech.SalesforceMarketingCloud.Models;

namespace bleak.Martech.SalesforceMarketingCloud.Sfmc.Rest.DataExtensions
{
    public interface IDataExtensionFolderRestApi
    {
        List<FolderObject> GetFolderTree();
        Task<List<FolderObject>> GetFolderTreeAsync();
    }
}