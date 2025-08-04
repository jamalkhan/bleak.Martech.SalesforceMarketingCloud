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
using bleak.Martech.SalesforceMarketingCloud.Api;
using bleak.Martech.SalesforceMarketingCloud.Api.Soap;
using SfmcApp.Pages.DataExtensions;
using bleak.Martech.SalesforceMarketingCloud.Sfmc.Rest.DataExtensions;

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


		// Asset Folder API
		builder.Services.AddTransient<AssetFolderRestApi>();
		builder.Services.AddTransient<IAssetFolderRestApi, AssetFolderRestApi>();

		// Asset API
		builder.Services.AddTransient<AssetRestApi>();
		builder.Services.AddTransient<IAssetRestApi, AssetRestApi>();
		
		// Data Extension Folder API
		builder.Services.AddTransient<DataExtensionFolderSoapApi>();
		builder.Services.AddTransient<IDataExtensionFolderApi, DataExtensionFolderSoapApi>();

		// Data Extension API
		builder.Services.AddTransient<DataExtensionSoapApi>();
		builder.Services.AddTransient<IDataExtensionApi, DataExtensionSoapApi>();
		

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

		// Asset List Page
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
					config: sfmcConnectionConfiguration,
					logger: folderApiLogger
				);
				var objectApi = new AssetRestApi(
					authRepository: authRepository,
					config: sfmcConnectionConfiguration,
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


		// Data Extension List Page
		builder.Services.AddTransient<Func<SfmcConnection, SfmcDataExtensionListPage2>>
		(
			sp => connection =>
			{
				var viewModelLogger = sp.GetRequiredService<ILogger<SfmcDataExtensionListViewModel>>();
				var logger = sp.GetRequiredService<ILogger<SfmcDataExtensionListPage2>>();
				var folderApiLogger = sp.GetRequiredService<ILogger<DataExtensionFolderSoapApi>>();
				var objectApiLogger = sp.GetRequiredService<ILogger<DataExtensionSoapApi>>();
				var restApiLogger = sp.GetRequiredService<ILogger<DataExtensionRestApi>>();
				var authRepoFactory = sp.GetRequiredService<Func<SfmcConnection, IAuthRepository>>();
				var authRepository = authRepoFactory(connection);
				var sfmcConnectionConfiguration = new SfmcConnectionConfiguration();
				var folderApi = new DataExtensionFolderSoapApi
				(
					authRepository: authRepository,
					config: sfmcConnectionConfiguration,
					logger: folderApiLogger
				);
				var objectApi = new DataExtensionSoapApi
				(
					authRepository: authRepository,
					config: sfmcConnectionConfiguration,
					logger: objectApiLogger
				);
				var dataExtensionRestApi = new DataExtensionRestApi
				(
					authRepository: authRepository,
					config: sfmcConnectionConfiguration,
					logger: restApiLogger
				);
				return new SfmcDataExtensionListPage2
				(
					sfmcConnection: connection,
					logger: viewModelLogger,
					folderApi: folderApi,
					objectApi: objectApi,
					restApi: dataExtensionRestApi
				);
			}
		);
		
		builder.Services.AddSingleton<SfmcConnectionConfiguration>();
		


#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
