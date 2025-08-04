using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Sfmc.Soap;

namespace bleak.Martech.SalesforceMarketingCloud.Api;

public interface IDataExtensionFolderApi
{
    List<DataExtensionFolder> GetFolderTree();
    Task<List<DataExtensionFolder>> GetFolderTreeAsync();
}
