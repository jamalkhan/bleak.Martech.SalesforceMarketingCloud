namespace SfmcApp;

using Microsoft.Extensions.Logging;
using SfmcApp.Models;
using System.Diagnostics;
using System.Text.Json;

public partial class SfmcConnectionEditPage : ContentPage
{
	const string ConnectionsPrefKey = "SfmcConnections";
    private ILogger<SfmcConnectionEditPage>? _logger => 
        App.Current?.Handler?.MauiContext?.Services?.GetService<ILogger<SfmcConnectionEditPage>>();

    public SfmcConnectionEditPage(SfmcConnection? connection = null)
    {
        InitializeComponent();
        if (connection == null)
        {
            IdEntry.Text = Guid.NewGuid().ToString();
        }
        else if (connection != null)
        {
            IdEntry.Text = connection.Id.ToString();
            NameEntry.Text = connection.Name;
            SubdomainEntry.Text = connection.Subdomain;
            ClientIdEntry.Text = connection.ClientId;
            ClientSecretEntry.Text = connection.ClientSecret;
            MemberIdEntry.Text = connection.MemberId;
        }
            try
            {
                _logger?.LogInformation("Initialized page");
            }
            catch (Exception ex)
            {
                DisplayAlert($"FUCK", $"Logger failed..", "Fuck");
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



        try
        {
            _logger?.LogInformation("Saving connection with Id: {Id}", id);
        }
        catch (Exception ex)
        {
            await DisplayAlert($"FUCK", $"Logger failed.. {ex}", "Fuck");
        }

        foreach (var conn in connections)
        {
            if (conn.Id == Guid.Empty)
            {
                conn.Id = Guid.NewGuid();
                _logger?.LogInformation("Saving NEW connection with Id: {Id}", id);
            }
            if (conn.Id == id)
            {
                _logger?.LogInformation("Overwriting existing gconnection with Id: {Id}", id);
                isFound = true;
                conn.Name = NameEntry.Text;
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
                Name = NameEntry.Text,
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