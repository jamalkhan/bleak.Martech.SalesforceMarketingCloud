using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using SfmcApp.Models;
using System.Text.Json;


namespace SfmcApp.ViewModels.Connections;

public class SfmcConnectionsListViewModel : BaseViewModel
{
    const string ConnectionsPrefKey = "SfmcConnections";

    private readonly ILogger<SfmcConnectionsListViewModel> _logger;
    public ObservableCollection<SfmcConnection> Connections { get; } = new();

    public SfmcConnectionsListViewModel(ILogger<SfmcConnectionsListViewModel> logger)
    {
        _logger = logger;
        LoadConnections();
        _logger.LogInformation("SfmcConnectionsListViewModel initialized");
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load saved connections.");
            }
        }
    }
}