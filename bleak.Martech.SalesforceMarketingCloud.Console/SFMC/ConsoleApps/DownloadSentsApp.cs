using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.Authentication;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Configuration;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Sfmc.Soap;
using bleak.Martech.SalesforceMarketingCloud.Fileops;

namespace bleak.Martech.SalesforceMarketingCloud.ConsoleApp.ConsoleApps
{
    public class DownloadSentsApp : IConsoleApp
    {
        public IRestClientAsync _restClientAsync { get; private set; }
        public IAuthRepository _authRepository { get; private set; }
        public string Folder { get;private set;}
        public int DaysBack { get; private set; }
        public DownloadSentsApp(
            IRestClientAsync restClientAsync,
            IAuthRepository authRepository, string folder, int daysBack = 180)
        {
            _restClientAsync = restClientAsync;
            _authRepository = authRepository;
            Folder = folder;
            DaysBack = daysBack;
        }

        public async Task Execute()
        {
            EnsureFolderExists();

            var startDate = DateTime.Today.AddDays(-DaysBack);
            var endDate = DateTime.Today;

            var dates = Enumerable.Range(0, (endDate - startDate).Days + 1)
                                .Select(offset => startDate.AddDays(offset))
                                .OrderBy(d => d)
                                .ToList();

            var semaphore = new SemaphoreSlim(AppConfiguration.Instance.MaxDegreeOfParallelism);

            var tasks = dates.Select(async date =>
            {
                await semaphore.WaitAsync();
                try
                {
                    await ProcessDateAsync(date);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(tasks);
        }


        private void EnsureFolderExists()
        {
            if (Directory.Exists(Folder))
            {
                Console.WriteLine($"✔ Folder exists: {Folder}");
                return;
            }

            Console.WriteLine($"⚠ Folder does not exist: {Folder}");
            Console.Write("Would you like to create it? (y/n): ");
            
            string? response = Console.ReadLine()?.Trim().ToLower();
            if (response == "y")
            {
                try
                {
                    Directory.CreateDirectory(Folder);
                    Console.WriteLine($"✅ Folder created: {Folder}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Failed to create folder: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("❌ Folder creation skipped.");
            }
        }

        private async Task ProcessDateAsync(DateTime startTime)
        {
            var endTime = startTime.AddDays(1);
            string path = Path.Combine(Folder, $"sends_{startTime:yyyyMMdd}_{endTime:yyyyMMdd}.csv");
            Console.WriteLine($"Downloading Sends for {startTime:yyyy-MM-dd} through {endTime:yyyy-MM-dd} to file {path}");

            
            var api = new SentEventSoapApi
            (
                restClientAsync: _restClientAsync,
                authRepository: _authRepository,
                fileWriter: new DelimitedFileWriter
                (
                    options: new DelimitedFileWriterOptions { Delimiter = "," }
                ),
                logger: null,
                startDate: startTime,
                endDate: endTime
            );

            await api.LoadDataSetAsync(filePath: path);
            
            Console.WriteLine($"Downloaded Sends for {startTime:yyyy-MM-dd} through {endTime:yyyy-MM-dd}");
        }
    }
}
