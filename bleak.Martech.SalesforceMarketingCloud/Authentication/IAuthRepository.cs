namespace bleak.Martech.SalesforceMarketingCloud.Authentication
{
    public interface IAuthRepository
    {
        string Subdomain { get; }
        Task<SfmcAuthToken> GetTokenAsync();
        Task ResolveAuthenticationAsync();
    }
}