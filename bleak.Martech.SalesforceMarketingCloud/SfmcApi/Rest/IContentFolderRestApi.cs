using bleak.Martech.SalesforceMarketingCloud.Models;

namespace bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Sfmc.Rest.Content
{
    public interface IContentFolderRestApi
    {
        List<FolderObject> GetFolderTree();
        Task<List<FolderObject>> GetFolderTreeAsync();
    }
}