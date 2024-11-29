using System;
using System.Text.Json;

namespace bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Configuration
{
    public partial class  AppConfiguration
    {
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
        
    }
}