using SfmcApp.Models;

namespace SfmcApp;

public partial class SfmcInstanceMenuPage : ContentPage
{
	public SfmcInstanceMenuPage(SfmcConnection connection)
	{
		InitializeComponent();
		BindingContext = connection;
	}
	
	public async void OnShowDataExtensionsClicked(object sender, EventArgs e)
	{
		//await Navigation.PushAsync(new SfmcDataExtensionListPage());
	}
}