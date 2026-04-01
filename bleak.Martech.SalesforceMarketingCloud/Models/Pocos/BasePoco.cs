namespace bleak.Martech.SalesforceMarketingCloud.Models.Pocos
{
    public abstract class BasePoco : IPoco
    {
        public string CustomerKey { get; set; } = string.Empty;
    }
}
