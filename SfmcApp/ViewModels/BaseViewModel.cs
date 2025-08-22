using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using SfmcApp.ViewModels.Services;

namespace SfmcApp.ViewModels;

public abstract class BaseViewModel<T>
{
    #region INotifyPropertyChanged implementation
    public event PropertyChangedEventHandler? PropertyChanged;
    public bool SetProperty<TProp>(ref TProp backingStore, TProp value, [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<TProp>.Default.Equals(backingStore, value))
            return false;

        backingStore = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
    #endregion INotifyPropertyChanged



    #region Logger
    protected readonly ILogger<T> _logger;
    #endregion Logger
    protected readonly INavigationService _navigationService;

    public BaseViewModel
    (
        INavigationService navigationService,
        ILogger<T> logger
    )
    {
        _logger = logger;
        _navigationService = navigationService;
    }
}
