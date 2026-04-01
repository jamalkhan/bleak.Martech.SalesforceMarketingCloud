namespace bleak.Martech.SalesforceMarketingCloud.Models.Sfmc.Soap;

public class ObjectDefinition
{
    public string ObjectType { get; set; } = string.Empty;
    public List<ObjectDefinitionProperty> Properties { get; set; } = [];
}
