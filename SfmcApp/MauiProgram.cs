using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using SfmcApp.Logging;
using bleak.Martech.SalesforceMarketingCloud.Sfmc.Rest.Assets;
using bleak.Martech.SalesforceMarketingCloud.Authentication;
using bleak.Martech.SalesforceMarketingCloud.Sfmc.Models;
using SfmcApp.Pages.Assets;
using bleak.Martech.SalesforceMarketingCloud.Configuration;
using SfmcApp.Models;
using SfmcApp.ViewModels;
using bleak.Martech.SalesforceMarketingCloud.Sfmc.Rest.DataExtensions;
using SfmcApp.Pages.DataExtensions;

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
		builder.Services.AddTransient<Func<SfmcConnection, IAuthRepository>>(sp => connection =>
		{
			var logger = sp.GetRequiredService<ILogger<MauiAuthRepository>>();
			return new MauiAuthRepository(connection, logger);
		});

		// Asset View Model
		builder.Services.AddTransient<SfmcAssetListViewModel>();

		builder.Services.AddSingleton<SfmcConnectionConfiguration>();

		// APIs

		// Asset Folder API
		builder.Services.AddTransient<AssetFolderRestApi>();
		builder.Services.AddTransient<IAssetFolderRestApi, AssetFolderRestApi>();

		// Asset API
		builder.Services.AddTransient<AssetRestApi>();
		builder.Services.AddTransient<IAssetRestApi, AssetRestApi>();

		// Data Extension Folder API
		builder.Services.AddTransient<DataExtensionFolderRestApi>();
		builder.Services.AddTransient<IDataExtensionFolderRestApi, DataExtensionFolderRestApi>();

		// Data Extension API
		builder.Services.AddTransient<DataExtensionRestApi>();
		builder.Services.AddTransient<IDataExtensionRestApi, DataExtensionRestApi>();

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


		// SfmcAssetListPage
		builder.Services.AddTransient<Func<SfmcConnection, SfmcAssetListPage>>
		(
			sp => connection =>
			{
				var viewModelLogger = sp.GetRequiredService<ILogger<SfmcAssetListViewModel>>();
				var logger = sp.GetRequiredService<ILogger<SfmcAssetListPage>>();
				var folderApiLogger = sp.GetRequiredService<ILogger<AssetFolderRestApi>>();
				var objectApiLogger = sp.GetRequiredService<ILogger<AssetRestApi>>();
				var authRepoFactory = sp.GetRequiredService<Func<SfmcConnection, IAuthRepository>>();
				var authRepository = authRepoFactory(connection);
				var sfmcConnectionConfiguration = new SfmcConnectionConfiguration();
				var folderApi = new AssetFolderRestApi(
					authRepository: authRepository,
					sfmcConnectionConfiguration: sfmcConnectionConfiguration,
					logger: folderApiLogger
				);
				var objectApi = new AssetRestApi(
					authRepository: authRepository,
					sfmcConnectionConfiguration: sfmcConnectionConfiguration,
					logger: objectApiLogger
				);
				return new SfmcAssetListPage(
					sfmcConnection: connection,
					logger: viewModelLogger,
					folderApi: folderApi,
					objectApi: objectApi
					);
			}
		);
		
		/*
		// SfmcDataExtensionListPage
		builder.Services.AddTransient<Func<SfmcConnection, SfmcDataExtensionListPage>>
		(
			sp => connection =>
			{
				var viewModelLogger = sp.GetRequiredService<ILogger<SfmcDataExtensionListViewModel>>();
				var pageLogger = sp.GetRequiredService<ILogger<SfmcDataExtensionListPage>>();
				var folderApiLogger = sp.GetRequiredService<ILogger<DataExtensionFolderRestApi>>();
				var objectApiLogger = sp.GetRequiredService<ILogger<DataExtensionRestApi>>();
				var authRepoFactory = sp.GetRequiredService<Func<SfmcConnection, IAuthRepository>>();
				var authRepository = authRepoFactory(connection);
				var sfmcConnectionConfiguration = new SfmcConnectionConfiguration();
				var folderApi = new DataExtensionFolderRestApi(
					authRepository: authRepository,
					sfmcConnectionConfiguration: sfmcConnectionConfiguration,
					logger: folderApiLogger
				);
				var objectApi = new DataExtensionRestApi(
					authRepository: authRepository,
					sfmcConnectionConfiguration: sfmcConnectionConfiguration,
					logger: objectApiLogger
				);
				return new SfmcDataExtensionListPage(
					sfmcConnection: connection,
					logger: viewModelLogger,
					folderApi: folderApi,
					objectApi: objectApi
					);
			}
		);
		*/
		


#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
