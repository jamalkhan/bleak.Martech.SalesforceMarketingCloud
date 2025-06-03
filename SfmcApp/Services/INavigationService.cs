namespace SfmcApp.ViewModels.Services
{
    public interface INavigationService
    {
        Task NavigateToAsync<TPage>() where TPage : Page;
    }
    public class NavigationService : INavigationService
    {
        private readonly IServiceProvider _provider;

        public NavigationService(IServiceProvider provider)
        {
            _provider = provider;
        }

        public async Task NavigateToAsync<TPage>() where TPage : Page
        {
            var page = _provider.GetRequiredService<TPage>();
            var nav = Application.Current?.MainPage?.Navigation;
            if (nav != null)
                await nav.PushAsync(page);
        }
    }
}