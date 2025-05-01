namespace bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Sfmc.Soap
{
    public interface IPoco 
    {

    }

    public abstract class BasePoco : IPoco
    {
        public string CustomerKey { get; set; } = string.Empty;
    }
}