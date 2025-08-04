namespace SfmcApp.Models.ViewModels;

public class DataExtensionViewModel
{
    public long CategoryId { get; set; } = 0;
    public string ObjectID { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string CustomerKey { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsSendable { get; set; } = false;
    public bool IsTestable { get; set; } = false;
}