using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.Authentication;
using bleak.Martech.SalesforceMarketingCloud.Sfmc.Models;
using SfmcApp.Models;
using SfmcApp.Pages;
using Microsoft.Extensions.Logging;
using SfmcApp.Pages.Assets;
using SfmcApp.Pages.DataExtensions;

namespace SfmcApp;

public partial class SfmcInstanceMenuPage : ContentPage
{
	public ILogger<SfmcInstanceMenuPage> _logger { get; private set; }
	public IAuthRepository _authRepository { get; private set; }
	public static JsonSerializer _serializer = new JsonSerializer();
	public static RestManager _restManager = new RestManager(_serializer, _serializer);
	public SfmcInstanceMenuPage(
		SfmcConnection connection,
		ILogger<SfmcInstanceMenuPage> logger
	)
	{
		InitializeComponent();
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
	public async void OnShowAssetClicked(object sender, EventArgs e)
	{
		if (App.Current?.Services is IServiceProvider services)
		{
			if (BindingContext is SfmcConnection connection)
			{
				var factory = services.GetRequiredService<Func<SfmcConnection, SfmcAssetListPage>>();
				_logger.LogInformation("Creating SfmcContentListPage with connection");
				var page = factory(connection);
				await Navigation.PushAsync(page);
			}
			else
			{
				_logger.LogError("BindingContext is not SfmcConnection");
			}
		}
	}

}