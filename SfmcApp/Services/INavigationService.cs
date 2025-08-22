using SfmcApp.Pages.DataExtensions;

namespace SfmcApp.ViewModels.Services
{
    public interface INavigationService
    {
        Task NavigateToAsync<TPage>() where TPage : Page;
        Task NavigateToFileImportAsync(string filePath);
    }

    public class NavigationService : INavigationService
    {
        private readonly IServiceProvider _serviceProvider;

        public NavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task NavigateToAsync<TPage>() where TPage : Page
        {
            var page = _serviceProvider.GetRequiredService<TPage>();

            // Get the first (main) window
            var window = Application.Current?.Windows.FirstOrDefault();
            if (window?.Page is Page rootPage)
            {
                var nav = rootPage.Navigation;
                if (nav != null)
                {
                    await nav.PushAsync(page);
                }
            }
        }

        public async Task NavigateToFileImportAsync(string filePath)
        {
            var page = _serviceProvider.GetRequiredService<SfmcDataExtensionFileImportPage>();

            // pass the parameter to the VM (not to the page ctor)
            if (page.BindingContext is SfmcDataExtensionFileImportViewModel vm)
            {
                await vm.InitializeAsync(filePath); // implement this in your VM
            }

            var window = Application.Current?.Windows.FirstOrDefault();
            if (window?.Page is Page rootPage)
            {
                await rootPage.Navigation.PushModalAsync(page);
            }
        }
    }
}