using SfmcApp.Models;
using Microsoft.Extensions.Logging;
using SfmcApp.Pages.Assets;
using SfmcApp.Pages.DataExtensions;

namespace SfmcApp;

public partial class SfmcInstanceMenuPage : ContentPage
{
	private readonly ILogger<SfmcInstanceMenuPage> _logger;

	public SfmcInstanceMenuPage(
		SfmcConnection connection,
		ILogger<SfmcInstanceMenuPage> logger
	)
	{
		InitializeComponent();
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		BindingContext = connection;
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

	public async void OnShowDataExtensionsClicked(object sender, EventArgs e)
	{
		if (App.Current?.Services is IServiceProvider services)
		{
			if (BindingContext is SfmcConnection connection)
			{
				var factory = services.GetRequiredService<Func<SfmcConnection, SfmcDataExtensionListPage>>();
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
}
