using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Configuration;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.ConsoleApps;
using bleak.Martech.SalesforceMarketingCloud.Authentication;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Sfmc.Soap;
using System.Diagnostics;
using bleak.Martech.SalesforceMarketingCloud.Configuration;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Authentication;
using Microsoft.Extensions.Logging;
using NLog;
using bleak.Martech.SalesforceMarketingCloud.Api.Soap;

namespace bleak.Martech.SalesforceMarketingCloud.ConsoleApp;

public static class Program
{
    private static ILogger<Process> _logger = (ILogger<Process>)LogManager.GetCurrentClassLogger();


    static JsonSerializer serializer = new JsonSerializer();
    static IRestClientAsync _restClient = new RestClient();
    static IAuthRepository _authRepository = new AuthRepository(
        restClientAsync: _restClient,
        subdomain: AppConfiguration.Instance.Subdomain,
        clientId: AppConfiguration.Instance.ClientId,
        clientSecret: AppConfiguration.Instance.ClientSecret,
        memberId: AppConfiguration.Instance.MemberId
    );
    

    private async static Task Main(string[] args)
    {

        try
        {
            _logger.LogInformation("Application starting...");

            await Task.Delay(500); // Simulate async work



            // See https://aka.ms/new-console-template for more information
            Console.WriteLine($"Getting Auth Token");
            await _authRepository.ResolveAuthenticationAsync();

            if (AppConfiguration.Instance.Debug) Console.WriteLine($"Gotten Auth Token {_authRepository.Token.access_token}");



            bool cont = true;
            while (cont)
            {
                Console.WriteLine("Which Operation?");
                Console.WriteLine("1. Content");
                Console.WriteLine("2. Data Extension Folders");
                Console.WriteLine("3. Data Extensions");
                Console.WriteLine("4. Data Extension Full Path File");
                Console.WriteLine("5. Shared Data Extension Full Path File");
                Console.WriteLine("6. Download All QueryDefinitions");
                Console.WriteLine("7. Download Opens for last 180 days");
                Console.WriteLine("8. Download Clicks for last 180 days");
                Console.WriteLine("9. Download Sents for last 180 days");
                Console.WriteLine("10. Download Images");
                var input = Console.ReadLine();

                var stopwatch = new Stopwatch();
                stopwatch.Start();

                Console.WriteLine($"You have entered {input}");

                switch (input)
                {
                    case "1":
                        var downloadContent = new DownloadContentApp(_restClient, _authRepository);
                        await downloadContent.Execute();
                        break;

                    case "2":
                        await Path2_DataExtensionFoldersAsync();
                        break;

                    case "3":
                        await Path3_DataExtensionsAsync();
                        break;

                    case "4":
                        await Path4_DataExtensionFullPathFileAsync();
                        break;
                    case "5":
                        await Path5_SharedDataExtensionFullPathFileAsync();
                        break;

                    case "6":
                        var queryDefinitionApp = new QueryDefinitionApp<QueryDefinitionPoco>(_authRepository);
                        await queryDefinitionApp.Execute();
                        break;

                    case "7":
                        var opensApp = new DownloadOpensApp(
                            restClientAsync: _restClient,
                            authRepository: _authRepository,
                            folder: Path.Combine(AppConfiguration.Instance.OutputFolder, "_SendTracking", "Opens"),
                            daysBack: 180
                        );
                        await opensApp.Execute();
                        break;

                    case "8":
                        var sentsApp = new DownloadSentsApp(
                            restClientAsync: _restClient,
                            authRepository: _authRepository,
                            folder: Path.Combine(AppConfiguration.Instance.OutputFolder, "_SentTracking", "Sents"),
                            daysBack: 180
                        );
                        await sentsApp.Execute();
                        break;

                    case "9":
                        var clicksApp = new DownloadSentsApp(
                            restClientAsync: _restClient,
                            authRepository: _authRepository,
                            folder: Path.Combine(AppConfiguration.Instance.OutputFolder, "_SentTracking", "Sents"),
                            daysBack: 180
                        );
                        await clicksApp.Execute();
                        break;

                    case "10":
                        var downloadImages = new DownloadImagesApp(_restClient, _authRepository);
                        await downloadImages.Execute();
                        break;

                    default:
                        {
                            Console.WriteLine("key not recognized. exiting...");
                            cont = false;
                            break;
                        }
                }

                // Stop the Stopwatch
                stopwatch.Stop();

                // Display the elapsed time
                Console.WriteLine($"Execution Time: {stopwatch.ElapsedMilliseconds} ms");
            }
        


            _logger.LogInformation("Application finished successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred.");
            throw;
        }
        finally
        {
            // Ensure logs are flushed before app exits
            LogManager.Shutdown();
        }
    }

    private static async Task Path2_DataExtensionFoldersAsync()
    {
        var lf2 = new DataExtensionFolderSoapApi(
            restClientAsync: _restClient,
            authRepository: _authRepository,
            config: new SfmcConnectionConfiguration(),
            logger: (ILogger<DataExtensionFolderSoapApi>)LogManager.GetLogger("DataExtensionFolderSoapApi")
        );
        var ft2 = await lf2.GetFolderTreeAsync();

        RenderFolderTree(ft2);
    }

    private async static Task Path3_DataExtensionsAsync()
    {
        var deapi = new DataExtensionSoapApi(
            restClientAsync: _restClient,
            authRepository: _authRepository,
            config: new SfmcConnectionConfiguration(),
            logger: (ILogger<DataExtensionSoapApi>)LogManager.GetLogger("DataExtensionFolderSoapApi")
            );
        var des = await deapi.GetAllDataExtensionsAsync();
        foreach (var de in des)
        {
            Console.WriteLine($"Data Extension: {de.Name}; category id: {de.CategoryID}");
        };
    }

    private async static Task Path4_DataExtensionFullPathFileAsync()
    {
        var lf = new DataExtensionFolderSoapApi
        (
            restClientAsync: _restClient,
            authRepository: _authRepository,
            config: new SfmcConnectionConfiguration(),
            logger: (ILogger<DataExtensionFolderSoapApi>)LogManager.GetLogger("DataExtensionFolderSoapApi")
        );

        var folderTree = await lf.GetFolderTreeAsync();
        var deapi = new DataExtensionSoapApi
        (
            restClientAsync: _restClient,
            authRepository: _authRepository,
            config: new SfmcConnectionConfiguration(),
            logger: (ILogger<DataExtensionSoapApi>)LogManager.GetLogger("DataExtensionSoapApi")
        );
        var dataExtensions = await deapi.GetAllDataExtensionsAsync();

        AddDEsToFolder(folderTree, dataExtensions);
        WriteOutFolderTree(folderTree);
    }

    private async static Task Path5_SharedDataExtensionFullPathFileAsync()
    {
        var lf = new SharedDataExtensionFolderSoapApi(
            restClientAsync: _restClient,
            authRepository: _authRepository,
            config: new SfmcConnectionConfiguration(),
            logger: (ILogger<SharedDataExtensionFolderSoapApi>)LogManager.GetLogger("SharedDataExtensionFolderSoapApi")
        );
        var folderTree = await lf.GetFolderTreeAsync();

        var deapi = new DataExtensionSoapApi(
            restClientAsync: _restClient,
            authRepository: _authRepository,
            config: new SfmcConnectionConfiguration(),
            logger: (ILogger<DataExtensionSoapApi>)LogManager.GetLogger("DataExtensionSoapApi")
        );
        var dataExtensions = await deapi.GetAllDataExtensionsAsync();

        AddDEsToFolder(folderTree, dataExtensions);
        WriteOutFolderTree(folderTree);
    }

    private static void WriteOutFolderTree(List<DataExtensionFolder> folderTree)
    {
        var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        string path = Path.Combine(desktopPath, "DEs.txt");
        Console.WriteLine($"Y / N Write to {path}?");
        var input = Console.ReadLine();
        if (input!.ToLower() == "y")
        {
            // Open a file stream with StreamWriter
            using (StreamWriter writer = new StreamWriter(path))
            {
                foreach (var folder in folderTree)
                {
                    WriteFolderContents(writer, folder);
                }
            }

            Console.WriteLine("File written successfully.");
        }
    }

    private static void WriteFolderContents(StreamWriter writer, DataExtensionFolder folder)
    {
        writer.WriteLine($"Folder: {folder.FullPath}");
        foreach (var de in folder.DataExtensions)
        {
            writer.WriteLine($"Data Extension: {de.FullPath}");
        }
        if (folder.SubFolders.Any())
        {
            foreach (var subfolder in folder.SubFolders)
            {
                WriteFolderContents(writer, subfolder);
            }
        }
    }

    private static void AddDEsToFolder(List<DataExtensionFolder> folderTree, List<DataExtensionPoco> allDataExtensions)
    {
        foreach (var folder in folderTree)
        {
            var dataExtensionsInFolder = allDataExtensions.Where(x => x.CategoryID == folder.Id).ToList();
            
            foreach (var de in dataExtensionsInFolder)
            {
                de.FullPath = folder.FullPath + "/" + de.Name;
                
            }

            folder.DataExtensions = dataExtensionsInFolder;
            var subfolders = folder.SubFolders;

            if (subfolders.Count > 0)
            {
                AddDEsToFolder(subfolders, allDataExtensions);
            }
        };
    }

    private static void RenderFolderTree(List<DataExtensionFolder> ft2)
    {
        foreach (var folder in ft2)
        {
            Console.WriteLine($"Folder: {folder.FullPath}");
            if (folder.SubFolders.Count > 0)
            {
                RenderFolderTree(folder.SubFolders);
            }
        }
    }
}