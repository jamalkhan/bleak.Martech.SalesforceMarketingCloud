using System;
using System.Text.Json;

namespace bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Configuration
{
    public partial class  AppConfiguration
    {
        public string OutputFolder { get;set; } = string.Empty;
        public string Subdomain { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string MemberId { get; set; } = string.Empty;
    }
}