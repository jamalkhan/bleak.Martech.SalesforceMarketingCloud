using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using SfmcApp.ViewModels;

namespace SfmcApp.ViewModels;

public class BaseViewModel<T>
{
    #region INotifyPropertyChanged implementation
    public event PropertyChangedEventHandler? PropertyChanged;
    public bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(backingStore, value))
            return false;

        backingStore = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
    #endregion INotifyPropertyChanged



    #region Logger
    protected readonly ILogger<T> _logger;
    #endregion Logger

    public BaseViewModel(ILogger<T> logger)
    {
        _logger = logger;
    }
}

public class BaseFolderAndListViewModel<T> : BaseViewModel<T>
{
    public BaseFolderAndListViewModel(ILogger<T> logger)
        : base(logger)
    {
    }

    
}
