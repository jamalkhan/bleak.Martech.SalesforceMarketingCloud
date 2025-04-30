using System.Collections.ObjectModel;
using System.Text.Json;
using SfmcApp.Models;

namespace SfmcApp;

public partial class SfmcConnectionListPage : ContentPage
{
    const string ConnectionsPrefKey = "SfmcConnections";

    public ObservableCollection<SfmcConnection> Connections { get; set; } = new();

    public SfmcConnectionListPage()
    {
        InitializeComponent();
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
                        Connections.Add(connection);
                }
            }
            catch
            {
                DisplayAlert("Error", "Failed to load saved connections.", "OK");
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
            await Navigation.PushAsync(new SfmcInstanceMenuPage(connection));
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
}
