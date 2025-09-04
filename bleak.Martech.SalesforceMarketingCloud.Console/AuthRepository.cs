using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.Authentication;

namespace bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Authentication;

public partial class AuthRepository : AuthRepositoryBase
{
    readonly JsonSerializer _jsonSerializer = new();
    
    private static readonly string AuthFilePath = Path.Combine(AppContext.BaseDirectory, "authentication.json");

    public AuthRepository(IRestClientAsync restClientAsync, string subdomain, string clientId, string clientSecret, string memberId)
        : base(restClientAsync, subdomain, clientId, clientSecret, memberId)
    {
    }

    protected override bool IsTokenValid()
    {
        if (File.Exists(AuthFilePath))
        {
            DateTime lastWriteTime = File.GetLastWriteTime(AuthFilePath);
            TimeSpan timeDifference = DateTime.Now - lastWriteTime;
            if (timeDifference.TotalSeconds <= Threshold)
            {
                return true;
            }
            File.Delete(AuthFilePath);
        }
        return false;
    }

    protected override SfmcAuthToken LoadToken()
    {
        if (File.Exists(AuthFilePath))
        {
            return _jsonSerializer.Deserialize<SfmcAuthToken>(File.ReadAllText(AuthFilePath));
        }
        throw new InvalidOperationException("No valid authentication file found.");
    }
    protected override async Task SaveTokenAsync(SfmcAuthToken token)
    {
        string json = _jsonSerializer.Serialize(token);
        await File.WriteAllTextAsync(AuthFilePath, json);
    }
}