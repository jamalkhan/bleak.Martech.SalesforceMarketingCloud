using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.Api;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Sfmc.Soap;
using bleak.Martech.SalesforceMarketingCloud.Fileops;
using bleak.Martech.SalesforceMarketingCloud.Sfmc.Rest.DataExtensions;
using Microsoft.Extensions.Logging;
using SfmcApp.Models;
using SfmcApp.Models.ViewModels;
using SfmcApp.ViewModels.Services;

namespace SfmcApp.ViewModels;

public class ColumnDefinition
{
    public string Name { get; set; }
    public string DataType { get; set; } // string, int, float, datetime
    public bool IsNullable { get; set; }
    public string SampleValues { get; set; }
}

public partial class SfmcDataExtensionFileImportViewModel
    : BaseSfmcFolderAndListViewModel
        <
            SfmcDataExtensionFileImportViewModel,
            FolderViewModel,
            IDataExtensionFolderApi,
            DataExtensionViewModel,
            IDataExtensionApi
        >
    , INotifyPropertyChanged
{

    public DataExtensionRestApi RestApi { get; }


    public ObservableCollection<ColumnDefinition> Columns { get; } = new();
    public string FileName { get; private set; }
    public string DataExtensionName { get; set; }
    public string CustomerKey { get; set; }

    public SfmcDataExtensionFileImportViewModel
    (
        INavigationService navigationService,
        SfmcConnection sfmcConnection,
        ILogger<SfmcDataExtensionFileImportViewModel> logger,
        IDataExtensionFolderApi folderApi,
        IDataExtensionApi contentResourceApi,
        DataExtensionRestApi deRestApi
    )
       : base
       (
           navigationService: navigationService,
           logger: logger,
           sfmcConnection: sfmcConnection,
           folderApi: folderApi,
           contentResourceApi: contentResourceApi,
           resourceType: "DataExtensions"
       )
    {
        RestApi = deRestApi;
    }

    public async Task InitializeAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path is required.");

        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        if (ext != ".csv" && ext != ".tsv")
            throw new InvalidOperationException("Only CSV or TSV files are supported.");

        FileName = Path.GetFileName(filePath);

        var delimiter = ext == ".csv" ? ',' : '\t';
        var lines = await File.ReadAllLinesAsync(filePath);

        if (lines.Length == 0)
            throw new InvalidOperationException("File is empty.");

        var headers = lines[0].Split(delimiter);
        var sampleRows = lines.Skip(1).Take(10).ToList();

        Columns.Clear();

        for (int i = 0; i < headers.Length; i++)
        {
            var colName = headers[i].Trim();
            var samples = sampleRows
                .Select(row => row.Split(delimiter))
                .Where(parts => parts.Length > i && !string.IsNullOrWhiteSpace(parts[i]))
                .Select(parts => parts[i])
                .Take(3)
                .ToList();

            var inferredType = InferType(samples);

            Columns.Add(new ColumnDefinition
            {
                Name = colName,
                DataType = inferredType,
                IsNullable = samples.Count < sampleRows.Count, // crude nullability check
                SampleValues = string.Join(", ", samples)
            });
        }
    }
    
        private string InferType(List<string> samples)
    {
        if (samples.All(s => int.TryParse(s, out _)))
            return "int";
        if (samples.All(s => float.TryParse(s, out _)))
            return "float";
        if (samples.All(s => DateTime.TryParse(s, out _)))
            return "datetime";
        return "string";
    }

    public override Task LoadContentResourcesForSelectedFolderAsync()
    {
        throw new NotImplementedException();
    }

    public override Task<IEnumerable<FolderViewModel>> GetFolderTreeAsync()
    {
        throw new NotImplementedException();
    }
}