using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.Authentication;
using Microsoft.Extensions.Logging;

namespace bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Authentication;

public partial class AuthRepository : AuthRepositoryBase
{
    readonly JsonSerializer _jsonSerializer = new();
    
    private static readonly string AuthFilePath = Path.Combine(AppContext.BaseDirectory, "authentication.json");

    public AuthRepository(
        IRestClientAsync restClientAsync,
        string subdomain,
        string clientId,
        string clientSecret,
        string memberId,
        string? authBaseUrl = null,
        ILogger<AuthRepository>? logger = null)
        : base(restClientAsync, subdomain, clientId, clientSecret, memberId, authBaseUrl, logger)
    {
    }

    protected override Task<AuthTokenState?> LoadTokenStateAsync()
    {
        if (File.Exists(AuthFilePath))
        {
            var token = _jsonSerializer.Deserialize<SfmcAuthToken>(File.ReadAllText(AuthFilePath));
            var storedAt = new DateTimeOffset(File.GetLastWriteTimeUtc(AuthFilePath));
            return Task.FromResult<AuthTokenState?>(new AuthTokenState(token, storedAt));
        }

        return Task.FromResult<AuthTokenState?>(null);
    }

    protected override async Task SaveTokenStateAsync(SfmcAuthToken token, DateTimeOffset storedAt)
    {
        string json = _jsonSerializer.Serialize(token);
        await File.WriteAllTextAsync(AuthFilePath, json);
        File.SetLastWriteTimeUtc(AuthFilePath, storedAt.UtcDateTime);
    }

    protected override Task ClearTokenStateAsync()
    {
        if (File.Exists(AuthFilePath))
        {
            File.Delete(AuthFilePath);
        }

        return Task.CompletedTask;
    }
}
