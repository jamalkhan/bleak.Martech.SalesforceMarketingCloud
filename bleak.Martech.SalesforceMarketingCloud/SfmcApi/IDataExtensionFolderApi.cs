using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Sfmc.Soap;

namespace bleak.Martech.SalesforceMarketingCloud.Api;

public interface IDataExtensionFolderApi
{
    Task<List<DataExtensionFolder>> GetFolderTreeAsync();
}
