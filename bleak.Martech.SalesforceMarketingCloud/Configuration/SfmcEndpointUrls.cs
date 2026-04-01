namespace bleak.Martech.SalesforceMarketingCloud.Configuration;

public static class SfmcEndpointUrls
{
    public static string GetAuthTokenUrl(string subdomain, string? authBaseUrl = null)
    {
        var baseUrl = string.IsNullOrWhiteSpace(authBaseUrl)
            ? $"https://{subdomain}.auth.marketingcloudapis.com"
            : authBaseUrl;

        return $"{TrimTrailingSlash(baseUrl)}/v2/token";
    }

    public static string GetRestEndpoint(string subdomain, string path, string? restBaseUrl = null)
    {
        var baseUrl = string.IsNullOrWhiteSpace(restBaseUrl)
            ? $"https://{subdomain}.rest.marketingcloudapis.com"
            : restBaseUrl;

        return $"{TrimTrailingSlash(baseUrl)}/{path.TrimStart('/')}";
    }

    public static string GetSoapServiceUrl(string subdomain, string? soapBaseUrl = null)
    {
        var baseUrl = string.IsNullOrWhiteSpace(soapBaseUrl)
            ? $"https://{subdomain}.soap.marketingcloudapis.com"
            : soapBaseUrl;

        var normalized = TrimTrailingSlash(baseUrl);
        return normalized.EndsWith("/Service.asmx", StringComparison.OrdinalIgnoreCase)
            ? normalized
            : $"{normalized}/Service.asmx";
    }

    public static string GetSoapToAddress(string subdomain, string? soapBaseUrl = null)
    {
        return GetSoapServiceUrl(subdomain, soapBaseUrl);
    }

    public static string GetSoapHost(string subdomain, string? soapBaseUrl = null)
    {
        return new Uri(GetSoapServiceUrl(subdomain, soapBaseUrl)).Authority;
    }

    private static string TrimTrailingSlash(string value)
    {
        return value.TrimEnd('/');
    }
}
