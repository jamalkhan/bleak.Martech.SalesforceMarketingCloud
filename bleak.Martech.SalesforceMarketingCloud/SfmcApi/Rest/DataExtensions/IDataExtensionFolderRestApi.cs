using bleak.Martech.SalesforceMarketingCloud.Models;

namespace bleak.Martech.SalesforceMarketingCloud.Sfmc.Rest.DataExtensions
{
    public interface IDataExtensionFolderRestApi
    {
        Task<List<FolderObject>> GetFolderTreeAsync();
    }
}