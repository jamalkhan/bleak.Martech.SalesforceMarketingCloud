using bleak.Martech.SalesforceMarketingCloud.Models.Pocos;

namespace bleak.Martech.SalesforceMarketingCloud.Sfmc.Rest.Assets
{
    public interface IAssetRestApi
    {        
        AssetPoco GetAsset(int? assetId, string? customerKey, string? name);
        Task<AssetPoco> GetAssetAsync(int? assetId, string? customerKey, string? name);
        List<AssetPoco> GetAssets(int folderId);
        Task<List<AssetPoco>> GetAssetsAsync(int folderId);
    }
}