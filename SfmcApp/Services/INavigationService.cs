using SfmcApp.Pages.DataExtensions;
using SfmcApp.Models.ViewModels;
using SfmcApp.Models;

namespace SfmcApp.ViewModels.Services
{
    public interface INavigationService
    {
        Task NavigateToAsync<TPage>() where TPage : Page;
        Task NavigateToFileImportAsync(SfmcConnection connection, string filePath, FolderViewModel selectedFolder);
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

        public async Task NavigateToFileImportAsync(SfmcConnection connection, string filePath, FolderViewModel selectedFolder)
        {
            var factory = _serviceProvider.GetRequiredService<Func<SfmcConnection, SfmcDataExtensionFileImportPage>>();
            var page = factory(connection);

            // pass the parameter to the VM (not to the page ctor)
            if (page.BindingContext is SfmcDataExtensionFileImportViewModel vm)
            {
                await vm.InitializeAsync(filePath, selectedFolder);
            }

            var window = Application.Current?.Windows.FirstOrDefault();
            if (window?.Page is Page rootPage)
            {
                await rootPage.Navigation.PushModalAsync(page);
            }
        }
    }
}
