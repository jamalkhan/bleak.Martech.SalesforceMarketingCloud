using bleak.Martech.SalesforceMarketingCloud.Models.Pocos;

namespace bleak.Martech.SalesforceMarketingCloud.Sfmc.Rest.Assets
{
    public interface IAssetRestApi
    {
        List<AssetPoco> GetAssets(int folderId);
        Task<List<AssetPoco>> GetAssetsAsync(int folderId);
    }
}