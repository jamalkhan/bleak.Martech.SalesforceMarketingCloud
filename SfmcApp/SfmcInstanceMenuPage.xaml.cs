using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.Authentication;
using SfmcApp.Models;
using SfmcApp.Pages;

namespace SfmcApp;

public partial class SfmcInstanceMenuPage : ContentPage
{
	public AuthRepository _authRepository { get; private set; }
	public static JsonSerializer _serializer = new JsonSerializer();
	public static RestManager _restManager = new RestManager(_serializer, _serializer);
	public SfmcInstanceMenuPage(SfmcConnection connection)
	{
		InitializeComponent();
		_authRepository = new AuthRepository(
			restManager: _restManager, 
			jsonSerializer: _serializer,
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
}