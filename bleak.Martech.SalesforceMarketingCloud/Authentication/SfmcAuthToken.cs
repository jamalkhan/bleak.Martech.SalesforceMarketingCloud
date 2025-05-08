using System;
using System.Text.Json;

namespace bleak.Martech.SalesforceMarketingCloud.Authentication
{
    public class SfmcAuthToken
    {
        public string access_token { get; set; } = string.Empty;
        public string token_type { get; set; } = "Bearer";
        public int expires_in { get; set; }
        public string scope { get; set; } = string.Empty;
        public string soap_instance_url { get; set; } = string.Empty;
        public string rest_instance_url { get; set; } = string.Empty;
    }
}