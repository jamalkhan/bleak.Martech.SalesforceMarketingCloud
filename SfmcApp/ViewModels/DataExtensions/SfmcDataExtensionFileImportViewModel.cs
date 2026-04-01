using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.Api;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Sfmc.Soap;
using bleak.Martech.SalesforceMarketingCloud.Fileops;
using bleak.Martech.SalesforceMarketingCloud.Sfmc.Rest.DataExtensions;
using bleak.Martech.SalesforceMarketingCloud.Models.Pocos;
using Microsoft.Extensions.Logging;
using SfmcApp.Models;
using SfmcApp.Models.ViewModels;
using SfmcApp.ViewModels.Services;
using Microsoft.VisualBasic.FileIO;

namespace SfmcApp.ViewModels;

public class ColumnDefinition
{
    public string Name { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty; // string, int, float, datetime
    public bool IsNullable { get; set; }
    public string SampleValues { get; set; } = string.Empty;
    public int? MaxLength { get; set; }
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
    public string FileName { get; private set; } = string.Empty;
    public string DataExtensionName { get; set; } = string.Empty;
    public string CustomerKey { get; set; } = string.Empty;
    public string TargetFolderName { get; private set; } = string.Empty;

    private bool _isImporting;
    public bool IsImporting
    {
        get => _isImporting;
        set => SetProperty(ref _isImporting, value);
    }

    private string _importStatus = string.Empty;
    public string ImportStatus
    {
        get => _importStatus;
        set => SetProperty(ref _importStatus, value);
    }

    private FolderViewModel? _targetFolder;
    private readonly List<Dictionary<string, string>> _rows = [];
    public ICommand ImportCommand { get; }

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
        ImportCommand = new Command(async () => await ImportAsync(), () => !IsImporting);
    }

    public async Task InitializeAsync(string filePath, FolderViewModel selectedFolder)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path is required.");

        _targetFolder = selectedFolder ?? throw new ArgumentNullException(nameof(selectedFolder));
        TargetFolderName = selectedFolder.Name;

        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        if (ext != ".csv" && ext != ".tsv")
            throw new InvalidOperationException("Only CSV or TSV files are supported.");

        FileName = Path.GetFileName(filePath);
        DataExtensionName = Path.GetFileNameWithoutExtension(filePath);
        CustomerKey = SanitizeCustomerKey(Path.GetFileNameWithoutExtension(filePath));
        ImportStatus = string.Empty;
        _rows.Clear();

        var delimiter = ext == ".csv" ? ',' : '\t';
        var parsedRows = await ParseDelimitedFileAsync(filePath, delimiter);

        if (parsedRows.Count == 0)
            throw new InvalidOperationException("File is empty.");

        var headers = parsedRows[0];
        var dataRows = parsedRows.Skip(1).ToList();

        if (headers.Length == 0)
            throw new InvalidOperationException("No headers were found in the file.");

        Columns.Clear();
        _rows.AddRange(ToRowDictionaries(headers, dataRows));

        for (int i = 0; i < headers.Length; i++)
        {
            var colName = headers[i].Trim();
            var samples = dataRows
                .Where(parts => parts.Length > i && !string.IsNullOrWhiteSpace(parts[i]))
                .Select(parts => parts[i])
                .Take(3)
                .ToList();

            var maxLength = dataRows
                .Where(parts => parts.Length > i && !string.IsNullOrWhiteSpace(parts[i]))
                .Select(parts => parts[i].Length)
                .DefaultIfEmpty(0)
                .Max();

            var inferredType = InferType(samples);

            Columns.Add(new ColumnDefinition
            {
                Name = colName,
                DataType = inferredType,
                IsNullable = dataRows.Any(parts => parts.Length <= i || string.IsNullOrWhiteSpace(parts[i])),
                MaxLength = maxLength == 0 ? null : maxLength,
                SampleValues = string.Join(", ", samples)
            });
        }

        ImportStatus = $"Loaded {_rows.Count} rows from {FileName}.";
    }
    
    private string InferType(List<string> samples)
    {
        if (samples.Count == 0)
            return "string";

        if (samples.All(s => bool.TryParse(s, out _)))
            return "bool";
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
        return Task.CompletedTask;
    }

    public override Task<IEnumerable<FolderViewModel>> GetFolderTreeAsync()
    {
        return Task.FromResult<IEnumerable<FolderViewModel>>(Enumerable.Empty<FolderViewModel>());
    }

    private async Task ImportAsync()
    {
        if (IsImporting)
            return;

        if (_targetFolder == null)
            throw new InvalidOperationException("A target folder is required.");

        if (string.IsNullOrWhiteSpace(DataExtensionName))
            throw new InvalidOperationException("Data extension name is required.");

        if (string.IsNullOrWhiteSpace(CustomerKey))
            throw new InvalidOperationException("Customer key is required.");

        if (_rows.Count == 0)
            throw new InvalidOperationException("There are no rows to import.");

        IsImporting = true;
        ImportStatus = "Creating data extension...";

        try
        {
            var definition = new DataExtensionImportDefinition
            {
                Name = DataExtensionName.Trim(),
                CustomerKey = CustomerKey.Trim(),
                Description = $"Imported from {FileName}",
                CategoryId = _targetFolder.Id,
                Columns = Columns.Select(column => new DataExtensionImportColumn
                {
                    Name = column.Name.Trim(),
                    DataType = column.DataType,
                    IsNullable = column.IsNullable,
                    MaxLength = column.MaxLength
                }).ToList()
            };

            await ContentResourceApi.CreateDataExtensionAsync(definition);

            ImportStatus = $"Created data extension. Importing {_rows.Count} rows...";
            var importedRows = await ContentResourceApi.AddRowsToDataExtensionAsync(definition.CustomerKey, _rows);
            ImportStatus = $"Import complete. Created '{definition.Name}' in '{TargetFolderName}' and inserted {importedRows} rows.";
        }
        catch (Exception ex)
        {
            ImportStatus = $"Import failed: {ex.Message}";
            _logger.LogError(ex, "Failed to import data extension file {FileName}", FileName);
        }
        finally
        {
            IsImporting = false;
        }
    }

    private static async Task<List<string[]>> ParseDelimitedFileAsync(string filePath, char delimiter)
    {
        var rows = new List<string[]>();

        using var parser = new TextFieldParser(filePath);
        parser.SetDelimiters(delimiter.ToString());
        parser.HasFieldsEnclosedInQuotes = delimiter == ',';

        while (!parser.EndOfData)
        {
            var fields = await Task.Run(() => parser.ReadFields() ?? []);
            rows.Add(fields);
        }

        return rows;
    }

    private static List<Dictionary<string, string>> ToRowDictionaries(string[] headers, IEnumerable<string[]> rows)
    {
        var dictionaries = new List<Dictionary<string, string>>();

        foreach (var row in rows)
        {
            var dict = new Dictionary<string, string>();

            for (var i = 0; i < headers.Length; i++)
            {
                var key = headers[i].Trim();
                var value = i < row.Length ? row[i] : string.Empty;
                dict[key] = value;
            }

            dictionaries.Add(dict);
        }

        return dictionaries;
    }

    private static string SanitizeCustomerKey(string input)
    {
        var sanitized = new string(input.Where(ch => char.IsLetterOrDigit(ch) || ch == '_' || ch == '-').ToArray());
        if (string.IsNullOrWhiteSpace(sanitized))
            sanitized = $"de_{Guid.NewGuid():N}";

        return sanitized.Length <= 36 ? sanitized : sanitized[..36];
    }
}
