using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.Authentication;
using Microsoft.Extensions.Logging;

namespace bleak.Martech.SalesforceMarketingCloud.Sfmc.Models;

public partial class MauiAuthRepository : AuthRepositoryBase
{
    public MauiAuthRepository(
        string subdomain,
        string clientId,
        string clientSecret,
        string memberId,
        string? authBaseUrl,
        IRestClientAsync restClientAsync,
        ILogger<MauiAuthRepository> logger)
        : base(
            restClientAsync: restClientAsync,
            subdomain: subdomain,
            clientId: clientId,
            clientSecret: clientSecret,
            memberId: memberId,
            authBaseUrl: authBaseUrl,
            logger: logger)
    {
    }
}
