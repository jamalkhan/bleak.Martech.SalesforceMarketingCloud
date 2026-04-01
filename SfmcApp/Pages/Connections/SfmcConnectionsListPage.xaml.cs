using System.Collections.ObjectModel;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using SfmcApp.Models;

namespace SfmcApp;

public partial class SfmcConnectionListPage : ContentPage
{
    const string ConnectionsPrefKey = "SfmcConnections";
    const string MockConnectionName = "Local SFMC Mock";
    const string MockBaseUrl = "http://127.0.0.1:5099";

    public ObservableCollection<SfmcConnection> Connections { get; set; } = new();

    private readonly ILogger<SfmcConnectionListPage> _logger;
    public SfmcConnectionListPage(
        ILogger<SfmcConnectionListPage> logger
    )
    {
        InitializeComponent();
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        BindingContext = this;
        LoadConnections();
    }

    private void LoadConnections()
    {
        Connections.Clear();
        var json = Preferences.Get(ConnectionsPrefKey, null);
        if (!string.IsNullOrEmpty(json))
        {
            try
            {
                var connections = JsonSerializer.Deserialize<List<SfmcConnection>>(json);
                if (connections != null)
                {
                    foreach (var connection in connections)
                    {
                        Connections.Add(connection);
                    }
                }
            }
            catch
            {
                _ = DisplayAlertAsync("Error", "Failed to load saved connections.", "OK");
            }
        }
    }

    private async void OnEditClicked(object sender, EventArgs e)
    {
        if ((sender as Button)?.CommandParameter is SfmcConnection connection)
        {
            // Optionally: pass the connection to the edit page
            await Navigation.PushAsync(new SfmcConnectionEditPage(connection));
        }
    }

    private async void OnConnectClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is SfmcConnection connection)
        {
            if (App.Current?.Services is IServiceProvider services)
            {
                _logger.LogTrace($"Preparing the SfmcInstanceMenuPage with connection: {connection.Name}");
                var factory = services.GetRequiredService<Func<SfmcConnection, SfmcInstanceMenuPage>>();
                var page = factory(connection);
                await Navigation.PushAsync(page);
            }
        }
    }

    private async void OnDeleteConnection(object sender, EventArgs e)
    {
        var swipeItem = sender as SwipeItem;
        if (swipeItem?.CommandParameter is SfmcConnection connToDelete)
        {
            Connections.Remove(connToDelete); // remove from in-memory list

            // Update Preferences
            var updatedJson = JsonSerializer.Serialize(Connections);
            Preferences.Set(ConnectionsPrefKey, updatedJson);
        }
    }

    private async void OnAddConnectionClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SfmcConnectionEditPage());
    }

    private async void OnAddMockConnectionClicked(object sender, EventArgs e)
    {
        var mockConnection = new SfmcConnection
        {
            Id = Guid.NewGuid(),
            Name = MockConnectionName,
            Subdomain = "mock",
            ClientId = "mock-client-id",
            ClientSecret = "mock-client-secret",
            MemberId = "123456",
            AuthBaseUrl = MockBaseUrl,
            RestBaseUrl = MockBaseUrl,
            SoapBaseUrl = MockBaseUrl,
        };

        var json = Preferences.Get(ConnectionsPrefKey, null);
        var connections = string.IsNullOrEmpty(json)
            ? new List<SfmcConnection>()
            : JsonSerializer.Deserialize<List<SfmcConnection>>(json) ?? new List<SfmcConnection>();

        var existing = connections.FirstOrDefault(connection =>
            string.Equals(connection.Name, MockConnectionName, StringComparison.OrdinalIgnoreCase)
            || (string.Equals(connection.AuthBaseUrl, MockBaseUrl, StringComparison.OrdinalIgnoreCase)
                && string.Equals(connection.RestBaseUrl, MockBaseUrl, StringComparison.OrdinalIgnoreCase)
                && string.Equals(connection.SoapBaseUrl, MockBaseUrl, StringComparison.OrdinalIgnoreCase)));

        if (existing is null)
        {
            connections.Add(mockConnection);
        }
        else
        {
            existing.Name = mockConnection.Name;
            existing.Subdomain = mockConnection.Subdomain;
            existing.ClientId = mockConnection.ClientId;
            existing.ClientSecret = mockConnection.ClientSecret;
            existing.MemberId = mockConnection.MemberId;
            existing.AuthBaseUrl = mockConnection.AuthBaseUrl;
            existing.RestBaseUrl = mockConnection.RestBaseUrl;
            existing.SoapBaseUrl = mockConnection.SoapBaseUrl;
        }

        Preferences.Set(ConnectionsPrefKey, JsonSerializer.Serialize(connections));
        LoadConnections();
        await DisplayAlertAsync("Mock Connection Added", $"Saved '{MockConnectionName}' pointing at {MockBaseUrl}.", "OK");
    }
}
