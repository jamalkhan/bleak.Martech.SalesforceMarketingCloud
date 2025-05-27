using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.Authentication;
using bleak.Martech.SalesforceMarketingCloud.Sfmc.Models;
using SfmcApp.Models;
using SfmcApp.Pages;
using SfmcApp.Pages.Content;

namespace SfmcApp;

public partial class SfmcInstanceMenuPage : ContentPage
{
	public IAuthRepository _authRepository { get; private set; }
	public static JsonSerializer _serializer = new JsonSerializer();
	public static RestManager _restManager = new RestManager(_serializer, _serializer);
	public SfmcInstanceMenuPage(SfmcConnection connection)
	{
		InitializeComponent();
		_authRepository = new MauiAuthRepository(
			subdomain: connection.Subdomain,
			clientId: connection.ClientId,
			clientSecret: connection.ClientSecret,
			memberId: connection.MemberId
		);
		BindingContext = connection;
	}

	public async void OnShowDataExtensionsClicked(object sender, EventArgs e)
	{
		await Navigation.PushAsync(new SfmcDataExtensionListPage(_authRepository));
	}
	public async void OnShowContentClicked(object sender, EventArgs e)
	{
		await Navigation.PushAsync(new SfmcContentListPage(_authRepository));
	}
}