using Microsoft.Extensions.Logging;
using System.Windows.Input;
using SfmcApp.ViewModels.Services;


namespace SfmcApp.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly INavigationService _navigation;
        private readonly ILogger<MainViewModel> _logger;
        public ICommand NavigateCommand { get; }

        public MainViewModel(INavigationService navigation, ILogger<MainViewModel> logger)
        {
            _navigation = navigation;
            _logger = logger;

            NavigateCommand = new Command(async () => await NavigateToSfmcConnectionListPage());

            _logger.LogInformation("MainViewModel initialized.");
        }



        private async Task NavigateToSfmcConnectionListPage()
        {
            _logger.LogInformation("Navigating to SfmcConnectionListPage.");
            await _navigation.NavigateToAsync<SfmcConnectionListPage>();
        }

    }

    public class BaseViewModel
    {
    }
}