using System.Reflection;

namespace bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Fileops
{
    public interface IFileWriter
    {
        string FilePath { get; }
        void WriteToFile<T>(IEnumerable<T> records);
    }

    public class DelimitedFileWriter : IFileWriter
    {
        private readonly object _fileLock = new object();

        public string FilePath { get; private set; }
        public DelimitedFileWriterOptions Options { get; private set; }
        public DelimitedFileWriter(string filePath, DelimitedFileWriterOptions options = null)
        {
            FilePath = filePath;
            Options = options;
        }

        public void WriteToFile<T>(IEnumerable<T> records)
        {
            if (records == null || !records.Any())
                return;

            Options ??= new DelimitedFileWriterOptions();
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                    .Where(p => p.GetValue(records.First()) != null)
                                    .ToArray();

            int attempt = 0;
            bool writeSuccessful = false;

            //if (AppConfiguration.Instance.Debug) Console.WriteLine($"[{this.GetType().Name} {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] Writing {records.Count()} records to {FilePath}");
            while (attempt < Options.MaxRetryAttempts && !writeSuccessful)
            {
                try
                {
                    lock (_fileLock)  // Ensure only one thread enters this block at a time
                    {
                        bool fileExists = File.Exists(FilePath);

                        using var stream = new FileStream(FilePath, FileMode.Append, FileAccess.Write, FileShare.Read);
                        using var writer = new StreamWriter(stream, Options.Encoding);

                        // Write header if file doesn't exist
                        if (!fileExists)
                        {
                            var header = string.Join(Options.Delimiter, properties.Select(p => p.Name));
                            writer.Write(header + Options.LineEnding);
                        }

                        // Write rows
                        foreach (var record in records)
                        {
                            var values = properties.Select(prop =>
                            {
                                var value = prop.GetValue(record);
                                if (value == null) return Options.NullPlaceholder;

                                string stringValue = value.ToString();
                                if (Options.TrimValues) stringValue = stringValue.Trim();

                                return Options.UseQuotes
                                    ? $"\"{stringValue.Replace("\"", "\"\"")}\"" // Escape quotes
                                    : stringValue;
                            });

                            writer.WriteLine(string.Join(Options.Delimiter, values));
                        }

                        writer.Flush(); // Ensure data is written
                        writeSuccessful = true; // Mark as successful
                        Console.WriteLine($"[{this.GetType().Name} {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] {FilePath} has been updated");
                    }
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"[{this.GetType().Name} {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] Error writing {FilePath}... {ex.Message}");
                    attempt++;
                    if (attempt < Options.MaxRetryAttempts)
                    {
                        Thread.Sleep(Options.RetryDelayMilliseconds); // Wait before retrying
                    }
                    else
                    {
                        throw; // If max attempts reached, propagate the error
                    }
                }
            }
        }
    }
}