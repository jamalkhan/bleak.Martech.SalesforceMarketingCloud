namespace bleak.Martech.SalesforceMarketingCloud.Models.Sfmc.Dtos.Soap;

public class ObjectDefinitionProperty
{
    public string? PartnerKey { get; set; }
    public string? ObjectID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public bool IsUpdatable { get; set; }
    public bool IsRetrievable { get; set; }
    public int? MaxLength { get; set; }
    public bool IsRequired { get; set; }
}