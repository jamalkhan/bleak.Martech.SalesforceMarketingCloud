using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.Authentication;
using bleak.Martech.SalesforceMarketingCloud.Sfmc.Models;
using SfmcApp.Models;
using SfmcApp.Pages;
using SfmcApp.Pages.Content;
using SfmcApp;
using Microsoft.Extensions.Logging;
using MetalPerformanceShadersGraph;

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
	public async void OnShowContentClicked(object sender, EventArgs e)
	{
		_logger.LogInformation("Navigating to Content List Page");
		var page = App.Current.Services.GetRequiredService<SfmcContentListPage>();
		_logger.LogInformation("And... Push");
		await Navigation.PushAsync(page);
		_logger.LogInformation("Pushed.");
	}
}