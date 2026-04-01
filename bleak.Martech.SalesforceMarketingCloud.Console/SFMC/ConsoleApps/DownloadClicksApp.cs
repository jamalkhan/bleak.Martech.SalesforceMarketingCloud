using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Configuration;
using bleak.Martech.SalesforceMarketingCloud.Authentication;
using bleak.Martech.SalesforceMarketingCloud.Fileops;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Sfmc.Soap;
using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.Models.Pocos;
using Microsoft.Extensions.Logging;

namespace bleak.Martech.SalesforceMarketingCloud.ConsoleApp.ConsoleApps
{
    public class DownloadClicksApp : IConsoleApp
    {
        public IRestClientAsync _restClientAsync { get; private set; }
        public IAuthRepository _authRepository { get; private set; }
        public ILogger<ClickEventSoapApi> Logger { get; }
        public string Folder { get;private set;}
        public int DaysBack { get; private set; }
        public DownloadClicksApp(
            IRestClientAsync restClientAsync,
            IAuthRepository authRepository,
            string folder,
            int daysBack = 180,
            ILogger<ClickEventSoapApi>? logger = null)
        {
            _restClientAsync = restClientAsync;
            _authRepository = authRepository;
            Folder = folder;
            DaysBack = daysBack;
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

            var tasks = dates.Select(date => ProcessDateAsync(date));

            await Task.WhenAll(tasks); // <-- properly awaits all async operations
        }

        private void EnsureFolderExists()
        {
            if (Directory.Exists(Folder))
            {
                Logger.LogInformation("Download folder exists. Path={Folder}", Folder);
                return;
            }

            Logger.LogWarning("Download folder does not exist. Path={Folder}", Folder);
            Console.Write("Would you like to create it? (y/n): ");
            
            string? response = Console.ReadLine()?.Trim().ToLower();
            if (response == "y")
            {
                try
                {
                    Directory.CreateDirectory(Folder);
                    Logger.LogInformation("Download folder created. Path={Folder}", Folder);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Failed to create download folder. Path={Folder}", Folder);
                }
            }
            else
            {
                Logger.LogWarning("Folder creation skipped by user. Path={Folder}", Folder);
            }
        }

        private async Task ProcessDateAsync(DateTime startTime)
        {
            var endTime = startTime.AddDays(1);
            string path = Path.Combine(Folder, $"sends_{startTime:yyyyMMdd}_{endTime:yyyyMMdd}.csv");
            Logger.LogInformation("Downloading clicks. StartDate={StartDate}, EndDate={EndDate}, Path={Path}", startTime, endTime, path);
            
            var api = new ClickEventSoapApi
            (
                restClientAsync: _restClientAsync,
                authRepository: _authRepository,
                fileWriter: new DelimitedFileWriter
                (
                    options: new DelimitedFileWriterOptions { Delimiter = "," },
                    logger: Logger
                ), 
                logger: Logger,
                startDate: startTime,
                endDate: endTime
            );

            await api.LoadDataSetAsync(filePath: path);

            Logger.LogInformation("Completed clicks download. StartDate={StartDate}, EndDate={EndDate}, Path={Path}", startTime, endTime, path);
        }
    }
}
