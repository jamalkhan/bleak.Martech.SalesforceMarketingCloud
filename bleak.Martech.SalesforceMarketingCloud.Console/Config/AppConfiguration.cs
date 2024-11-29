using System;
using System.Text.Json;

namespace bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Configuration
{

    public class AppConfiguration
    {
            //public List<Integration> Integrations { get; set; }
            public string OutputFolder { get;set; } = string.Empty;
            public string Subdomain { get; set; } = string.Empty;
            public string ClientId { get; set; } = string.Empty;
            public string ClientSecret { get; set; } = string.Empty;
            public string MemberId { get; set; } = string.Empty;
            
            #region Base
            private static JsonSerializerOptions _options = new JsonSerializerOptions()
                {
                    Converters =
                    {
                        //new DataTypeConverter(),
                    }
                };
            private static AppConfiguration? _instance = null;
            private static object _syncLock = new object();
            public static AppConfiguration Instance
            {
                get
                {
                    if (_instance == null)
                    {
                        lock(_syncLock)
                        {
                            if (_instance == null)
                            {
                                var configText = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "config.json"));
                                _instance = JsonSerializer.Deserialize<AppConfiguration>(configText, _options) 
                                ?? throw new InvalidOperationException("Failed to deserialize AppConfiguration.");
                            }
                        }
                    }
                    return _instance!;
                }
            }
            #endregion Base
    }
}