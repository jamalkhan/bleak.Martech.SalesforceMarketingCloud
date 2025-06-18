namespace bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Sfmc.Soap
{
    public abstract class BasePoco : IPoco
    {
        public string CustomerKey { get; set; } = string.Empty;
    }
}