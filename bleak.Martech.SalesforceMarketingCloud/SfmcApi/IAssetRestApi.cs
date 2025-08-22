using bleak.Martech.SalesforceMarketingCloud.Models.Pocos;

namespace bleak.Martech.SalesforceMarketingCloud.Api;

public interface IAssetRestApi
{        
    Task<AssetPoco> GetAssetAsync(int? assetId = null, string? customerKey = null, string? name = null);
    Task<IEnumerable<AssetPoco>> GetAssetsAsync(int folderId);
    Task<IEnumerable<AssetPoco>> SearchAssetsAsync(string searchTerm, int? folderId = null);

}
