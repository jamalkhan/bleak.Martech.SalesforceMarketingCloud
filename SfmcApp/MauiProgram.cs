using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using SfmcApp.Logging;

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

       
#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
