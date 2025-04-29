namespace SfmcApp;
using SfmcApp.Models;
using System.Text.Json;

public partial class SfmcConnectionEditPage : ContentPage
{

	const string ConnectionsPrefKey = "SfmcConnections";

	public SfmcConnectionEditPage()
	{
		InitializeComponent();
        IdEntry.Text = Guid.NewGuid().ToString();
    }

	public SfmcConnectionEditPage(SfmcConnection? connection = null)
	{
		InitializeComponent();

		if (connection != null)
		{
            IdEntry.Text = connection.Id.ToString();
			SubdomainEntry.Text = connection.Subdomain;
			ClientIdEntry.Text = connection.ClientId;
			ClientSecretEntry.Text = connection.ClientSecret;
			MemberIdEntry.Text = connection.MemberId;
		}
	}


    private async void OnSaveClicked(object sender, EventArgs e)
    {
        var id = Guid.Parse(IdEntry.Text);
        bool isFound = false;

        var json = Preferences.Get(ConnectionsPrefKey, null);
        var connections = string.IsNullOrEmpty(json)
            ? new List<SfmcConnection>()
            : JsonSerializer.Deserialize<List<SfmcConnection>>(json) ?? new List<SfmcConnection>();

        foreach (var conn in connections)
        {
            if (conn.Id == Guid.Empty)
            {
                DisplayAlert($"Found {SubdomainEntry.Text}", $"Id = {IdEntry.Text} Subdomain = {SubdomainEntry.Text} was not found.", "Fuck");
                conn.Id = Guid.NewGuid();
            }
            if (conn.Id == id)
            {
                DisplayAlert($"Found {SubdomainEntry.Text}", $"Id = {IdEntry.Text} Subdomain = {SubdomainEntry.Text} was found.", "OK");
                isFound = true;
                conn.Subdomain = SubdomainEntry.Text;
                conn.ClientId = ClientIdEntry.Text;
                conn.ClientSecret = ClientSecretEntry.Text;
                conn.MemberId = MemberIdEntry.Text;
            }
        }

        if (!isFound)
        {
            var newConnection = new SfmcConnection
            {
                Id = id,
                Subdomain = SubdomainEntry.Text,
                ClientId = ClientIdEntry.Text,
                ClientSecret = ClientSecretEntry.Text,
                MemberId = MemberIdEntry.Text
            };
            connections.Add(newConnection);
        }
        
        var updatedJson = JsonSerializer.Serialize(connections);
        Preferences.Set(ConnectionsPrefKey, updatedJson);

        await Navigation.PopAsync(); // Navigate back to the previous page

    }
}