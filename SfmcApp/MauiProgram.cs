using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using SfmcApp.Logging;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Sfmc.Rest.Content;
using bleak.Martech.SalesforceMarketingCloud.Authentication;
using bleak.Martech.SalesforceMarketingCloud.Sfmc.Models;
using SfmcApp.Pages.Content;
using bleak.Martech.SalesforceMarketingCloud.Configuration;
using SfmcApp.Models;

namespace SfmcApp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			})
			.UseMauiCommunityToolkit()
			;
		
		string logPath = Path.Combine(FileSystem.AppDataDirectory, "app.log");
        builder.Logging.AddProvider(new FileLoggerProvider(logPath));
        builder.Logging.SetMinimumLevel(LogLevel.Debug); // Or whatever level you want


		// Sfmc Connection Configuration
		builder.Services.AddTransient<SfmcConnectionConfiguration>();

		// Auth Repository
		builder.Services.AddTransient<MauiAuthRepository>();
		builder.Services.AddTransient<IAuthRepository, MauiAuthRepository>();

		// Content Folder API
		builder.Services.AddSingleton<ContentFolderRestApi>();
		builder.Services.AddSingleton<IContentFolderRestApi, ContentFolderRestApi>();

		// Pages
		builder.Services.AddTransient<MainPage>();
		builder.Services.AddTransient<SfmcConnectionEditPage>();
		builder.Services.AddTransient<SfmcConnectionListPage>();
		builder.Services.AddTransient<SfmcInstanceMenuPage>();



		// This lets the DI container resolve everything except SfmcConnection
		// which you provide at runtime.
		builder.Services.AddTransient<Func<SfmcConnection, SfmcInstanceMenuPage>>(sp => connection =>
		{
			var logger = sp.GetRequiredService<ILogger<SfmcInstanceMenuPage>>();
			return new SfmcInstanceMenuPage(connection, logger);
		});



		builder.Services.AddTransient<SfmcContentListPage>();
		builder.Services.AddSingleton<SfmcConnectionConfiguration>();
		builder.Services.AddTransient<ContentFolderRestApi>();


#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
