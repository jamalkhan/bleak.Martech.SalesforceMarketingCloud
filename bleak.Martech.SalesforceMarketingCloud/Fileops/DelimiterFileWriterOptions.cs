using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace bleak.Martech.SalesforceMarketingCloud.Fileops
{
    public class DelimitedFileWriterOptions
    {
        public string Delimiter { get; set; } = ",";  // Default to CSV
        public bool UseQuotes { get; set; } = true;   // Default to quoted
        public Encoding Encoding { get; set; } = Encoding.UTF8;
        public bool TrimValues { get; set; } = false;
        public string NullPlaceholder { get; set; } = string.Empty;
        public string LineEnding { get; set; } = Environment.NewLine;
        public int MaxRetryAttempts { get; set; } = 5; // Retries in case of file lock
        public int RetryDelayMilliseconds { get; set; } = 100; // Delay between retries
    }
}