using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.Authentication;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Sfmc.Soap;

namespace bleak.Martech.SalesforceMarketingCloud.ConsoleApp.ConsoleApps
{
    public class QueryDefinitionApp<T> : IConsoleApp
        where T : QueryDefinitionPoco
    {
        IAuthRepository _authRepository;
        public QueryDefinitionApp(IAuthRepository authRepository)
        {
            _authRepository = authRepository;
        }

        public async Task Execute()
        {
            var lf = new QueryDefinitionSoapApi
            (
                restClientAsync: new RestClient(),
                authRepository: _authRepository,
                logger: null
            );
            var pocos = await lf.GetQueryDefinitionPocosAsync();
            WriteFile(pocos);
        }

        private static void WriteFile(List<QueryDefinitionPoco> pocos)
        {
            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string path = Path.Combine(desktopPath, "querydefinitions.html");
            Console.WriteLine($"Y / N Write to {path}?");
            var input = Console.ReadLine();
            if (input!.ToLower() == "y")
            {
                // Open a file stream with StreamWriter
                using (StreamWriter writer = new StreamWriter(path))
                {
                    writer.WriteLine($"<!DOCTYPE html>");
                    writer.WriteLine($"<html lang=\"en\">");
                    writer.WriteLine($"<head>");
                    writer.WriteLine($"    <meta charset=\"UTF-8\">");
                    writer.WriteLine($"    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
                    writer.WriteLine($"    <title>SQL Scripts</title>");
                    writer.WriteLine($"    <style>");
                    writer.WriteLine("        body {");
                    writer.WriteLine($"            font-family: Arial, sans-serif;");
                    writer.WriteLine($"            margin: 20px;");
                    writer.WriteLine($"            line-height: 1.6;");
                    writer.WriteLine("        }");
                    writer.WriteLine("        .sql-script {");
                    writer.WriteLine($"            border: 1px solid #ddd;");
                    writer.WriteLine($"            padding: 15px;");
                    writer.WriteLine($"            margin-bottom: 20px;");
                    writer.WriteLine($"            border-radius: 5px;");
                    writer.WriteLine($"            background-color: #f9f9f9;");
                    writer.WriteLine("        }");
                    writer.WriteLine("        .script-name {");
                    writer.WriteLine($"            font-weight: bold;");
                    writer.WriteLine($"            margin-bottom: 10px;");
                    writer.WriteLine($"            font-size: 1.2em;");
                    writer.WriteLine("        }");
                    writer.WriteLine("        pre {");
                    writer.WriteLine($"            background-color: #333;");
                    writer.WriteLine($"            color: #fff;");
                    writer.WriteLine($"            padding: 10px;");
                    writer.WriteLine($"            border-radius: 5px;");
                    writer.WriteLine($"            overflow-x: auto;");
                    writer.WriteLine($"            font-size: 0.9em;");
                    writer.WriteLine("        }");
                    writer.WriteLine($"    </style>");
                    writer.WriteLine($"</head>");
                    writer.WriteLine($"<body>");
                    writer.WriteLine($"    <h1>SQL Script Files</h1>");
                    writer.WriteLine($"");
                    foreach (var poco in pocos)
                    {
                        writer.WriteLine($"    <!-- Repeating block for each SQL script -->");
                        writer.WriteLine($"    <div class=\"sql-script\">");
                        writer.WriteLine($"        <div class=\"script-name\">{poco.Name}</div>");
                        writer.WriteLine($"        <div>Description: {poco.Description}</div>");
                        writer.WriteLine($"        <div>Data Extension Target Name: {poco.DataExtensionTargetName}</div>");
                        writer.WriteLine($"        <pre>");
                        writer.WriteLine($"{poco.QueryText}");
                        writer.WriteLine($"        </pre>");
                        writer.WriteLine($"    </div>");
                        writer.WriteLine($"");
                    }

                    writer.WriteLine($"</body>");
                    writer.WriteLine($"</html>");
                }

                Console.WriteLine("File written successfully.");
            }
        }
    }
}