using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Authentication;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Configuration;
using bleak.Martech.SalesforceMarketingCloud.ContentBuilder;
using bleak.Martech.SalesforceMarketingCloud.ContentBuilder.SfmcPocos;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Sfmc.Soap;
using System.Diagnostics;
using System;
using System.IO;

namespace bleak.Martech.SalesforceMarketingCloud.ConsoleApp.ConsoleApps
{
    public class DownloadOpensApp : IConsoleApp
    {
        public AuthRepository _authRepository { get; private set; }
        public string Folder { get;private set;}
        public int DaysBack { get; private set; }
        public DownloadOpensApp(AuthRepository authRepository, string folder, int daysBack = 180)
        {
            _authRepository = authRepository;
            Folder = folder;
            DaysBack = daysBack;
        }

        public void Execute()
        {
            var date = DateTime.Today.AddDays(-DaysBack);
            do
            {
                Console.WriteLine($"Downloading Opens for {date:yyyy-MM-dd}");
                var endDate = date.AddDays(1);
                var api = new Sfmc.Soap.OpenEventSoapApi(authRepository: _authRepository, startDate: date, endDate: endDate);
                var pocos = api.LoadDataSet();
                
                WriteToCSV(date, endDate, pocos);
                date = endDate;
            }   while (date < DateTime.Today);
        }

        private void WriteToCSV(DateTime startDate, DateTime endDate, List<OpenEventPoco> pocos)
        {
            try
            {
                
                string path = Path.Combine(Folder, $"opens_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.csv");
                Console.WriteLine($"Writing Opens to {path}");
                // Open a file stream with StreamWriter
                using (StreamWriter writer = new StreamWriter(path))
                {
                    writer.WriteLine($"\"SubscriberKey\",\"EventDate\",\"EventType\"");
                    foreach (var poco in pocos)
                    {
                        writer.WriteLine($"\"{poco.SubscriberKey}\",\"{poco.EventDate}\",\"{poco.EventType}\"");
                    }
                }
                Console.WriteLine($"Write Complete {path}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
