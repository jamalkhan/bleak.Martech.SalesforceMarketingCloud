using System.Collections.ObjectModel;

namespace SfmcApp;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
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