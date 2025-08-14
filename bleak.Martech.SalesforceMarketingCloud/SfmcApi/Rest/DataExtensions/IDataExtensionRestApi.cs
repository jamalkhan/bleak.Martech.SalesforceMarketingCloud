using bleak.Martech.SalesforceMarketingCloud.Fileops;

namespace bleak.Martech.SalesforceMarketingCloud.Sfmc.Rest.DataExtensions
{
    public interface IDataExtensionRestApi
    {
        Task<long> DownloadDataExtensionAsync(string dataExtensionCustomerKey, IFileWriter fileWriter, string fileName);
    }
}


