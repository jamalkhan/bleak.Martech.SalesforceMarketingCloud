namespace SfmcApp;

public partial class App : Application
{
	public App(IServiceProvider services)
	{
		InitializeComponent();
		Services = services;
	}

	public static new App Current => (App)Application.Current;
    public IServiceProvider Services { get; }

	protected override Window CreateWindow(IActivationState? activationState)
	{
		var mainPage = Current.Services.GetRequiredService<MainPage>();
		return new Window(new NavigationPage(mainPage));
	}
}
