namespace bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Sfmc.Soap.DataExtensions
{
    public class QueryDefinitionPoco : BasePoco
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string DataExtensionTargetName { get; set; } = string.Empty;
        public string QueryText { get; set; } = string.Empty;

    }
}