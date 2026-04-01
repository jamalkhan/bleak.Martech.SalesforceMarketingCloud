using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Sfmc.Soap;
using bleak.Martech.SalesforceMarketingCloud.Models.Pocos;

namespace bleak.Martech.SalesforceMarketingCloud.Api;

public interface IDataExtensionApi
{
    Task<List<DataExtensionPoco>> GetAllDataExtensionsAsync();
    Task<List<DataExtensionPoco>> GetDataExtensionsByFolderAsync(int folderId);
    Task<List<DataExtensionPoco>> GetDataExtensionsNameEndsWithAsync(string nameEndsWith);
    Task<List<DataExtensionPoco>> GetDataExtensionsNameLikeAsync(string nameLike);
    Task<List<DataExtensionPoco>> GetDataExtensionsNameStartsWithAsync(string nameStartsWith);
    Task CreateDataExtensionAsync(DataExtensionImportDefinition definition);
    Task<int> AddRowsToDataExtensionAsync(string customerKey, IReadOnlyList<Dictionary<string, string>> rows);
}
