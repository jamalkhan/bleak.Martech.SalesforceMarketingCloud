using System.ComponentModel;
using System.Runtime.CompilerServices;
using bleak.Martech.SalesforceMarketingCloud.Authentication;

namespace SfmcApp.Pages.BasePages;

public abstract partial class BasePage : ContentPage, INotifyPropertyChanged
{
    protected IAuthRepository _authRepository { get; private set; }
    
    public BasePage(IAuthRepository authRepository)
    {
        _authRepository = authRepository;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Add common logic for all pages when they appear
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        // Add common cleanup logic here
    }

    // You can also expose common methods for derived pages
    protected void ShowError(string message)
    {
        DisplayAlert("Error", message, "OK");
    }
}