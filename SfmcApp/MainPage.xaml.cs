using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;

namespace SfmcApp;

public partial class MainPage : ContentPage
{
    
    private readonly ILogger<MainPage> _logger;

	public MainPage()
    {
        InitializeComponent();
        //_logger = MauiApp.Cu
        BindingContext = this;
    }

    private async void OnConnectClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SfmcConnectionEditPage());
    }

    private async void OnShowSfmcConnectionsClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SfmcConnectionListPage());
    }
}