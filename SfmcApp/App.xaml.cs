namespace SfmcApp;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

		//MainPage = new AppShell();
	}

	protected override Window CreateWindow(IActivationState activationState)
    {
        // Create and return the main window with the root page
        return new Window(new MainPage());
    }
}
