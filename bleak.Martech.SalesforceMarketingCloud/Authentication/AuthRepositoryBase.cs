using bleak.Api.Rest;
using Microsoft.Extensions.Logging;

namespace bleak.Martech.SalesforceMarketingCloud.Authentication;

public abstract class AuthRepositoryBase : IAuthRepository
{
    protected const double ThresholdSeconds = 600.07;

    protected readonly IRestClientAsync _restClientAsync;
    protected readonly ILogger? _logger;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    private SfmcAuthToken? _cachedToken;
    private DateTimeOffset _cachedTokenTimestamp = DateTimeOffset.MinValue;
    private bool _hydratedFromStore;

    public string Subdomain { get; protected set; }
    protected string ClientId { get; set; }
    protected string ClientSecret { get; set; }
    protected string MemberId { get; set; }
    protected string AuthBaseUrl { get; set; }

    protected AuthRepositoryBase(
        IRestClientAsync restClientAsync,
        string subdomain,
        string clientId,
        string clientSecret,
        string memberId,
        string? authBaseUrl = null,
        ILogger? logger = null)
    {
        _restClientAsync = restClientAsync;
        Subdomain = subdomain;
        ClientId = clientId;
        ClientSecret = clientSecret;
        MemberId = memberId;
        AuthBaseUrl = authBaseUrl ?? string.Empty;
        _logger = logger;
    }

    public async Task<SfmcAuthToken> GetTokenAsync()
    {
        var token = await TryGetValidTokenAsync().ConfigureAwait(false);
        if (token != null)
        {
            return token;
        }

        await _semaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            token = await TryGetValidTokenAsync().ConfigureAwait(false);
            if (token != null)
            {
                _logger?.LogDebug("Another thread refreshed the SFMC token for subdomain {Subdomain}.", Subdomain);
                return token;
            }

            _logger?.LogInformation("SFMC token missing or expired for subdomain {Subdomain}. Re-authenticating.", Subdomain);
            token = await AuthenticateAsync().ConfigureAwait(false);
            SetInMemoryToken(token, GetCurrentTime());
            await SaveTokenStateAsync(token, _cachedTokenTimestamp).ConfigureAwait(false);
            return token;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task<SfmcAuthToken?> TryGetValidTokenAsync()
    {
        if (IsTokenFresh(_cachedToken, _cachedTokenTimestamp))
        {
            _logger?.LogDebug("Using cached SFMC authentication token for subdomain {Subdomain}.", Subdomain);
            return _cachedToken;
        }

        if (!_hydratedFromStore)
        {
            _hydratedFromStore = true;

            var persisted = await LoadTokenStateAsync().ConfigureAwait(false);
            if (persisted?.Token != null && IsTokenFresh(persisted.Token, persisted.StoredAt))
            {
                SetInMemoryToken(persisted.Token, persisted.StoredAt);
                _logger?.LogDebug("Loaded persisted SFMC authentication token for subdomain {Subdomain}.", Subdomain);
                return _cachedToken;
            }

            if (persisted != null)
            {
                _logger?.LogDebug("Persisted SFMC token was stale for subdomain {Subdomain}; clearing it.", Subdomain);
                await ClearTokenStateAsync().ConfigureAwait(false);
            }
        }

        return null;
    }

    protected virtual DateTimeOffset GetCurrentTime() => DateTimeOffset.UtcNow;

    protected virtual TimeSpan GetTokenRetentionThreshold() => TimeSpan.FromSeconds(ThresholdSeconds);

    protected virtual Task<AuthTokenState?> LoadTokenStateAsync() => Task.FromResult<AuthTokenState?>(null);

    protected virtual Task SaveTokenStateAsync(SfmcAuthToken token, DateTimeOffset storedAt) => Task.CompletedTask;

    protected virtual Task ClearTokenStateAsync() => Task.CompletedTask;

    protected bool IsTokenFresh(SfmcAuthToken? token, DateTimeOffset storedAt)
    {
        if (token == null || storedAt == DateTimeOffset.MinValue)
        {
            return false;
        }

        return (GetCurrentTime() - storedAt) <= GetTokenRetentionThreshold();
    }

    protected void SetInMemoryToken(SfmcAuthToken token, DateTimeOffset storedAt)
    {
        _cachedToken = token;
        _cachedTokenTimestamp = storedAt;
    }

    protected async Task<SfmcAuthToken> AuthenticateAsync()
    {
        string tokenUri = Configuration.SfmcEndpointUrls.GetAuthTokenUrl(Subdomain, AuthBaseUrl);
        _logger?.LogInformation("Authenticating to SFMC for subdomain {Subdomain} using {TokenUri}.", Subdomain, tokenUri);
        _logger?.LogTrace("Auth request details. MemberId={MemberId}, ClientIdSuffix={ClientIdSuffix}", MemberId, MaskClientId(ClientId));

        var authResults = await _restClientAsync.ExecuteRestMethodAsync<SfmcAuthToken, string>(
            uri: new Uri(tokenUri),
            verb: HttpVerbs.POST,
            payload: new
            {
                grant_type = "client_credentials",
                client_id = ClientId,
                client_secret = ClientSecret,
                account_id = MemberId,
            },
            headers: new List<Header>
            {
                new Header { Name = "Content-Type", Value = "application/json" }
            }
        ).ConfigureAwait(false);

        if (authResults.Error != null)
        {
            _logger?.LogError("SFMC authentication failed for subdomain {Subdomain}. Error={Error}", Subdomain, authResults.Error);
            throw new Exception($"Authentication failed: {authResults.Error}");
        }

        _logger?.LogInformation("SFMC authentication succeeded for subdomain {Subdomain}. Token expires in {ExpiresInSeconds} seconds.", Subdomain, authResults.Results?.expires_in);
        return authResults.Results ?? throw new InvalidOperationException("Authentication returned no token.");
    }

    protected sealed record AuthTokenState(SfmcAuthToken Token, DateTimeOffset StoredAt);

    private static string MaskClientId(string clientId)
    {
        if (string.IsNullOrEmpty(clientId))
        {
            return string.Empty;
        }

        return clientId.Length <= 4
            ? clientId
            : $"***{clientId[^4..]}";
    }
}
