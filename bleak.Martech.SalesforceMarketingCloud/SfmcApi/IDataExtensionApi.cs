using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Sfmc.Soap;

namespace bleak.Martech.SalesforceMarketingCloud.Api;

public interface IDataExtensionApi
{
    Task<List<DataExtensionPoco>> GetAllDataExtensionsAsync();
    Task<List<DataExtensionPoco>> GetDataExtensionsByFolderAsync(int folderId);
    Task<List<DataExtensionPoco>> GetDataExtensionsNameEndsWithAsync(string nameEndsWith);
    Task<List<DataExtensionPoco>> GetDataExtensionsNameLikeAsync(string nameLike);
    Task<List<DataExtensionPoco>> GetDataExtensionsNameStartsWithAsync(string nameStartsWith);
}