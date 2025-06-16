using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;

namespace SfmcApp;

public partial class MainPage : ContentPage
{
    
    private readonly ILogger<MainPage> _logger;

	public MainPage(ILogger<MainPage> logger)
    {
        InitializeComponent();
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _logger.LogInformation("MainPage initialized");
        BindingContext = this;
    }

    private async void OnConnectClicked(object sender, EventArgs e)
    {
        var page = App.Current.Services.GetRequiredService<SfmcConnectionEditPage>();
        await Navigation.PushAsync(page);

    }

    private async void OnShowSfmcConnectionsClicked(object sender, EventArgs e)
    {
        var page = App.Current.Services.GetRequiredService<SfmcConnectionListPage>();
        await Navigation.PushAsync(page);

    }
}