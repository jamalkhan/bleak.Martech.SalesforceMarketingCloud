using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Configuration;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.ConsoleApps;
using bleak.Martech.SalesforceMarketingCloud.Authentication;
using bleak.Martech.SalesforceMarketingCloud.Models;
using bleak.Martech.SalesforceMarketingCloud.Models.SfmcDtos;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Sfmc.Soap;
using System.Diagnostics;
using System;
using System.IO;
using bleak.Martech.SalesforceMarketingCloud.Configuration;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Authentication;

namespace bleak.Martech.SalesforceMarketingCloud.ConsoleApp
{
    public static class Program
    {
        static JsonSerializer serializer = new JsonSerializer();
        static RestManager _restManager = new RestManager(serializer, serializer);
        static IAuthRepository _authRepository = new AuthRepository(
            subdomain: AppConfiguration.Instance.Subdomain,
            clientId: AppConfiguration.Instance.ClientId,
            clientSecret: AppConfiguration.Instance.ClientSecret,
            memberId: AppConfiguration.Instance.MemberId
        );
        

        private static void Main(string[] args)
        {
            // See https://aka.ms/new-console-template for more information
            Console.WriteLine($"Getting Auth Token");
            _authRepository.ResolveAuthentication();

            if (AppConfiguration.Instance.Debug) Console.WriteLine($"Gotten Auth Token {_authRepository.Token.access_token}");

            

            bool cont = true;
            while (cont)
            {
                Console.WriteLine("Which Operation?");
                Console.WriteLine("1. Content");
                Console.WriteLine("2. Data Extension Folders");
                Console.WriteLine("3. Data Extensions");
                Console.WriteLine("4. Data Extension Full Path File");
                Console.WriteLine("5. Download All QueryDefinitions");
                Console.WriteLine("6. Download Opens for last 180 days");
                Console.WriteLine("7. Download Clicks for last 180 days");
                Console.WriteLine("8. Download Sents for last 180 days");
                Console.WriteLine("9. Download Images");
                var input = Console.ReadLine();

                var stopwatch = new Stopwatch();
                stopwatch.Start();

                Console.WriteLine($"You have entered {input}");

                switch (input)
                {
                    case "1":
                        var downloadContent = new DownloadContent(_restManager, _authRepository);
                        downloadContent.Execute();
                        break;

                    case "2":
                        Path2_DataExtensionFolders();
                        break;

                    case "3":
                        Path3_DataExtensions();
                        break;

                    case "4":
                        Path4_DataExtensionFullPathFile();
                        break;

                    case "5":
                        var queryDefinitionApp = new QueryDefinitionApp<QueryDefinitionPoco>(_authRepository);
                        queryDefinitionApp.Execute();
                        break;

                    case "6":
                        var opensApp = new DownloadOpensApp(
                            authRepository:_authRepository,
                            folder: Path.Combine(AppConfiguration.Instance.OutputFolder, "_SendTracking", "Opens"),
                            daysBack: 180
                        );
                        opensApp.Execute();
                        break;

                    case "7":
                        var sentsApp = new DownloadSentsApp(
                            authRepository:_authRepository,
                            folder: Path.Combine(AppConfiguration.Instance.OutputFolder, "_SentTracking", "Sents"),
                            daysBack: 180
                        );
                        sentsApp.Execute();
                        break;
                    
                    case "8":
                        var clicksApp = new DownloadSentsApp(
                            authRepository:_authRepository,
                            folder: Path.Combine(AppConfiguration.Instance.OutputFolder, "_SentTracking", "Sents"),
                            daysBack: 180
                        );
                        clicksApp.Execute();
                        break;

                    case "9":
                        var downloadImages = new DownloadImagesApp(_restManager, _authRepository);
                        downloadImages.Execute();
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
        }

        private static void Path2_DataExtensionFolders()
        {
            var lf2 = new Sfmc.Soap.DataExtensionFolderSoapApi(
                authRepository: _authRepository,
                config: new SfmcConnectionConfiguration()
                );
            var ft2 = lf2.GetFolderTree();

            RenderFolderTree(ft2);
        }

        private static void Path3_DataExtensions()
        {
            var deapi = new Sfmc.Soap.DataExtensionSoapApi(
                authRepository: _authRepository
                );
            var des = deapi.GetAllDataExtensions();
            foreach (var de in des)
            {
                Console.WriteLine($"Data Extension: {de.Name}; category id: {de.CategoryID}");
            };
        }

        private static void Path4_DataExtensionFullPathFile()
        {
            var lf = new Sfmc.Soap.DataExtensionFolderSoapApi(
                authRepository: _authRepository,
                config: new SfmcConnectionConfiguration());
            var folderTree = lf.GetFolderTree();

            var deapi = new Sfmc.Soap.DataExtensionSoapApi(authRepository: _authRepository);
            var dataExtensions = deapi.GetAllDataExtensions();

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
}