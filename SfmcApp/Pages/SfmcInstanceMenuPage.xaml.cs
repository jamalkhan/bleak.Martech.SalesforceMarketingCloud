using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.Authentication;
using bleak.Martech.SalesforceMarketingCloud.Sfmc.Models;
using SfmcApp.Models;
using Microsoft.Extensions.Logging;
using SfmcApp.Pages.Assets;
using SfmcApp.Pages.DataExtensions;

namespace SfmcApp;

public partial class SfmcInstanceMenuPage : ContentPage
{
	public ILogger<SfmcInstanceMenuPage> _logger { get; private set; }
	public IAuthRepository _authRepository { get; private set; }
	public static JsonSerializer _serializer = new JsonSerializer();
	static IRestClientAsync _restClientAsync { get; set; }

	public SfmcInstanceMenuPage(
		SfmcConnection connection,
		IRestClientAsync restManagerAsync,
		IAuthRepository authRepository,
		ILogger<SfmcInstanceMenuPage> logger
	)
	{
		InitializeComponent();
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_authRepository = authRepository ?? throw new ArgumentNullException(nameof(authRepository));
		_restClientAsync = restManagerAsync ?? throw new ArgumentNullException(nameof(restManagerAsync));
		BindingContext = connection;
	}

	public async void OnShowDataExtensionsClicked(object sender, EventArgs e)
	{
		await Navigation.PushAsync(new SfmcDataExtensionListPage
		(
			restClientAsync: _restClientAsync,
			authRepository: _authRepository,
			logger: null
		));
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

	public async void OnShowDataExtensions2Clicked(object sender, EventArgs e)
	{
		if (App.Current?.Services is IServiceProvider services)
		{
			if (BindingContext is SfmcConnection connection)
			{
				var factory = services.GetRequiredService<Func<SfmcConnection, SfmcDataExtensionListPage2>>();
				_logger.LogInformation("Creating SfmcDataExtensionListPage with connection");
				var page = factory(connection);
				await Navigation.PushAsync(page);
			}
			else
			{
				_logger.LogError("BindingContext is not SfmcConnection");
			}
		}
	}

	public async void OnNetworkTestClicked(object sender, EventArgs e)
	{
		try
        {
            _logger.LogInformation("Executing TestNetworkCommand...");
            bool isConnected = await ((MauiAuthRepository)_authRepository).TestNetworkConnectivityAsync();
            _logger.LogInformation($"Network test result: {isConnected}");
            // Update UI on main thread
            MainThread.BeginInvokeOnMainThread(() =>
			{
				// Update UI-bound properties here, e.g., ObservableProperty
				DisplayAlert("Success", $"Networking TEST A-OK", "OK");
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Network command failed: {Message}\n{StackTrace}", ex.Message, ex.StackTrace);
            MainThread.BeginInvokeOnMainThread(() =>
            {
                DisplayAlert("Error", $"Network command failed: {ex.Message}", "OK");
            });
        }
	}
}