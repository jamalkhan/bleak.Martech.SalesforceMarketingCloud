using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using SfmcApp.Logging;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Sfmc.Rest.Content;
using bleak.Martech.SalesforceMarketingCloud.Authentication;
using bleak.Martech.SalesforceMarketingCloud.Sfmc.Models;
using SfmcApp.Pages.Content;
using bleak.Martech.SalesforceMarketingCloud.Configuration;

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
		builder.Services.AddTransient<MainPage>();
		builder.Services.AddSingleton<ContentFolderRestApi>();
		builder.Services.AddTransient<IAuthRepository, MauiAuthRepository>();
		builder.Services.AddSingleton<IContentFolderRestApi, ContentFolderRestApi>();
		builder.Services.AddTransient<SfmcContentListPage>();
		builder.Services.AddSingleton<SfmcConnectionConfiguration>();
		builder.Services.AddTransient<ContentFolderRestApi>();


#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
