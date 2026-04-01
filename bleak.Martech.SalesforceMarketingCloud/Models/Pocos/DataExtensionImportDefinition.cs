namespace bleak.Martech.SalesforceMarketingCloud.Models.Pocos;

public class DataExtensionImportDefinition
{
    public string Name { get; set; } = string.Empty;
    public string CustomerKey { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public List<DataExtensionImportColumn> Columns { get; set; } = [];
}

public class DataExtensionImportColumn
{
    public string Name { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public bool IsNullable { get; set; } = true;
    public int? MaxLength { get; set; }
}
