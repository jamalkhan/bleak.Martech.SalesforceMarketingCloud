using bleak.Martech.SalesforceMarketingCloud.Authentication;

namespace bleak.Martech.SalesforceMarketingCloud.Sfmc.Models
{
    public partial class MauiAuthRepository : AuthRepositoryBase
    {
        private static Lazy<SfmcAuthToken>? _cachedToken = null;
        private static readonly string AuthFilePath = Path.Combine(AppContext.BaseDirectory, "authentication.json");

        public MauiAuthRepository(string subdomain, string clientId, string clientSecret, string memberId)
            : base(subdomain, clientId, clientSecret, memberId)
        {
            _cachedToken = new Lazy<SfmcAuthToken>(LoadToken, true); // Thread-safe lazy loading
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

        protected override void SaveToken(SfmcAuthToken token)
        {
            string json = _jsonSerializer.Serialize(token);
            File.WriteAllText(AuthFilePath, json);
        }
    }
}