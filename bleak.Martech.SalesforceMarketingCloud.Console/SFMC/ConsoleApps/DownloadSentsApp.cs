using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Authentication;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Configuration;
using bleak.Martech.SalesforceMarketingCloud.ContentBuilder;
using bleak.Martech.SalesforceMarketingCloud.ContentBuilder.SfmcPocos;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Sfmc.Soap;
using System.Diagnostics;
using System;
using System.IO;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Fileops;

namespace bleak.Martech.SalesforceMarketingCloud.ConsoleApp.ConsoleApps
{
    public class DownloadSentsApp : IConsoleApp
    {
        public AuthRepository _authRepository { get; private set; }
        public string Folder { get;private set;}
        public int DaysBack { get; private set; }
        public DownloadSentsApp(AuthRepository authRepository, string folder, int daysBack = 180)
        {
            _authRepository = authRepository;
            Folder = folder;
            DaysBack = daysBack;
        }

        public void Execute()
        {
            EnsureFolderExists();

            var startDate = DateTime.Today.AddDays(-DaysBack);
            var endDate = DateTime.Today;

            var dates = Enumerable.Range(0, (endDate - startDate).Days + 1)
                      .Select(offset => startDate.AddDays(offset))
                      .ToList();
            dates.Sort();


            Parallel.ForEach(
                    source: dates,
                    parallelOptions: new ParallelOptions { MaxDegreeOfParallelism = AppConfiguration.Instance.MaxDegreeOfParallelism },
                    body: date => ProcessDate(date)
                );
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
            
            string response = Console.ReadLine()?.Trim().ToLower();
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

        private void ProcessDate(DateTime startTime)
        {
            var endTime = startTime.AddDays(1);
            string path = Path.Combine(Folder, $"sends_{startTime:yyyyMMdd}_{endTime:yyyyMMdd}.csv");
            Console.WriteLine($"Downloading Sends for {startTime:yyyy-MM-dd} through {endTime:yyyy-MM-dd} to file {path}");
            
            var api = new Sfmc.Soap.SentEventSoapApi  
            (
                authRepository: _authRepository,
                fileWriter: new DelimitedFileWriter
                (
                    filePath: path,
                    options: new DelimitedFileWriterOptions { Delimiter = "," }
                ),
                startDate: startTime,
                endDate: endTime
            );

            api.LoadDataSet();
            
            Console.WriteLine($"Downloaded Sends for {startTime:yyyy-MM-dd} through {endTime:yyyy-MM-dd}");
        }
    }
}
