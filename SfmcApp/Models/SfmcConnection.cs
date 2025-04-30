namespace SfmcApp.Models;

public class SfmcConnection
{
    public Guid Id { get; set; }

    public string Name { get; set; }
    public string Subdomain { get; set; }
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string MemberId { get; set; }
}