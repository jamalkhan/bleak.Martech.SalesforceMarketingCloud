namespace bleak.Martech.SalesforceMarketingCloud.Authentication
{
    public interface IAuthRepository
    {
        string Subdomain { get; }
        SfmcAuthToken Token { get; }
        void ResolveAuthentication();
        Task ResolveAuthenticationAsync();
    }
}