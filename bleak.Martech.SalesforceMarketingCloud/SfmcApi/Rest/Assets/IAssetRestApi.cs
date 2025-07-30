using bleak.Martech.SalesforceMarketingCloud.Models.Pocos;

namespace bleak.Martech.SalesforceMarketingCloud.Sfmc.Rest.Assets
{
    public interface IAssetRestApi
    {        
        AssetPoco GetAsset(int? assetId = null, string? customerKey = null, string? name = null);
        Task<AssetPoco> GetAssetAsync(int? assetId = null, string? customerKey = null, string? name = null);
        List<AssetPoco> GetAssets(int folderId);
        Task<List<AssetPoco>> GetAssetsAsync(int folderId);
    }
}