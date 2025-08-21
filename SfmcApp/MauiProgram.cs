using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.Api;
using bleak.Martech.SalesforceMarketingCloud.Api.Soap;
using bleak.Martech.SalesforceMarketingCloud.Authentication;
using bleak.Martech.SalesforceMarketingCloud.Configuration;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Sfmc.Soap;
using bleak.Martech.SalesforceMarketingCloud.Sfmc.Models;
using bleak.Martech.SalesforceMarketingCloud.Sfmc.Rest.Assets;
using bleak.Martech.SalesforceMarketingCloud.Sfmc.Rest.DataExtensions;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using SfmcApp.Logging;
using SfmcApp.Models;
using SfmcApp.Pages.Assets;
using SfmcApp.Pages.DataExtensions;
using SfmcApp.ViewModels;

namespace SfmcApp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		try
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
			builder.Services.AddSingleton<SfmcConnectionConfiguration>();

			// Auth Repository
			builder.Services.AddTransient<Func<SfmcConnection, IAuthRepository>>(sp => connection =>
			{
				var logger = sp.GetRequiredService<ILogger<MauiAuthRepository>>();
				return new MauiAuthRepository
				(
					subdomain: connection.Subdomain,
					clientId: connection.ClientId,
					clientSecret: connection.ClientSecret,
					memberId: connection.MemberId,
					jsonSerializer: sp.GetRequiredService<JsonSerializer>(),
					restClientAsync: sp.GetRequiredService<IRestClientAsync>(),
					logger: logger
				);
			});

			// Asset View Model with factory for SfmcConnection dependency
			builder.Services.AddTransient<Func<SfmcConnection, SfmcAssetListViewModel>>(sp => connection =>
			{
				var logger = sp.GetRequiredService<ILogger<SfmcAssetListViewModel>>();
				var folderApiLogger = sp.GetRequiredService<ILogger<AssetFolderRestApi>>();
				var objectApiLogger = sp.GetRequiredService<ILogger<AssetRestApi>>();
				var sfmcConnectionConfiguration = sp.GetRequiredService<SfmcConnectionConfiguration>();
				
				var folderApi = new AssetFolderRestApi
				(
					restClientAsync: sp.GetRequiredService<RestClient>(),
					authRepository: sp.GetRequiredService<Func<SfmcConnection, IAuthRepository>>()(connection),
					config: sfmcConnectionConfiguration,
					logger: folderApiLogger
				);
				
				var objectApi = new AssetRestApi
				(
					restClientAsync: sp.GetRequiredService<RestClient>(),
					authRepository: sp.GetRequiredService<Func<SfmcConnection, IAuthRepository>>()(connection),
					config: sfmcConnectionConfiguration,
					logger: objectApiLogger
				);
				
				return new SfmcAssetListViewModel(connection, logger, folderApi, objectApi);
			});

			// RestManager
			builder.Services.AddTransient<JsonSerializer>();
			builder.Services.AddTransient<ISerializer, JsonSerializer>();
			builder.Services.AddTransient<IDeserializer, JsonSerializer>();
			builder.Services.AddTransient<RestClient>();
			builder.Services.AddTransient<IRestClientAsync, RestClient>();


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
			builder.Services.AddTransient<Func<SfmcConnection, SfmcInstanceMenuPage>>
			(
				sp => connection =>
				{
					var logger = sp.GetRequiredService<ILogger<SfmcInstanceMenuPage>>();
					return new SfmcInstanceMenuPage
					(
						connection: connection,
						restManagerAsync: sp.GetRequiredService<IRestClientAsync>(),
						authRepository: sp.GetRequiredService<Func<SfmcConnection, IAuthRepository>>()(connection),
						logger: logger
					);
				}
			);

			// Asset List Page - Simplified registration
			builder.Services.AddTransient<SfmcAssetListPage>();
			builder.Services.AddTransient<SfmcDataExtensionListPage2>();

			// Asset List Page with factory for SfmcConnection dependency
			builder.Services.AddTransient<Func<SfmcConnection, SfmcAssetListPage>>
			(
				sp => connection =>
				{
					var viewModelFactory = sp.GetRequiredService<Func<SfmcConnection, SfmcAssetListViewModel>>();
					var viewModel = viewModelFactory(connection);
					return new SfmcAssetListPage(viewModel);
				}
			);

			// Data Extension List Page with factory for SfmcConnection dependency
			builder.Services.AddTransient<Func<SfmcConnection, SfmcDataExtensionListPage2>>
			(
				sp => connection =>
				{
					var viewModelFactory = sp.GetRequiredService<Func<SfmcConnection, SfmcDataExtensionListViewModel>>();
					var viewModel = viewModelFactory(connection);
					return new SfmcDataExtensionListPage2(viewModel);
				}
			);

			// Data Extension View Model
			builder.Services.AddTransient<Func<SfmcConnection, SfmcDataExtensionListViewModel>>(sp => connection =>
			{
				var serializer = new SoapSerializer();

				var logger = sp.GetRequiredService<ILogger<SfmcDataExtensionListViewModel>>();
				var folderApiLogger = sp.GetRequiredService<ILogger<DataExtensionFolderSoapApi>>();
				var objectApiLogger = sp.GetRequiredService<ILogger<DataExtensionSoapApi>>();
				var restApiLogger = sp.GetRequiredService<ILogger<DataExtensionRestApi>>();

				var sfmcConnectionConfiguration = sp.GetRequiredService<SfmcConnectionConfiguration>();

    			// Get the RestClient once, pass in the SoapSerializer
    			var restClient = new RestClient
				(
					serializer: serializer,
					deserializer: serializer
				);


				var folderApi = new DataExtensionFolderSoapApi
				(
					restClientAsync: restClient,
					authRepository: sp.GetRequiredService<Func<SfmcConnection, IAuthRepository>>()(connection),
					config: sp.GetRequiredService<SfmcConnectionConfiguration>(),
					logger: folderApiLogger
				);

				var objectApi = new DataExtensionSoapApi
				(
					restClientAsync: restClient,
					authRepository: sp.GetRequiredService<Func<SfmcConnection, IAuthRepository>>()(connection),
					config: sp.GetRequiredService<SfmcConnectionConfiguration>(),
					logger: objectApiLogger
				);
				var dataExtensionRestApi = new DataExtensionRestApi
				(
					restClientAsync: sp.GetRequiredService<RestClient>(),
					authRepository: sp.GetRequiredService<Func<SfmcConnection, IAuthRepository>>()(connection),
					config: sp.GetRequiredService<SfmcConnectionConfiguration>(),
					logger: restApiLogger
				);

				return new SfmcDataExtensionListViewModel
				(
					sfmcConnection:connection,
					logger:logger,
					folderApi:folderApi,
					contentResourceApi: objectApi,
					deRestApi: dataExtensionRestApi
				);
			}
			);
#if DEBUG
			builder.Logging.AddDebug();
#endif

			return builder.Build();
		}
		catch (Exception ex)
		{
        	System.Diagnostics.Debug.WriteLine("Startup crash: " + ex);
			throw;
		}
	}
}
