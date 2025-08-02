using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using SfmcApp.Models;
using SfmcApp.Models.ViewModels;
using SfmcApp.ViewModels;

namespace SfmcApp.ViewModels;

public abstract class BaseViewModel<T>
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

public abstract class BaseSfmcViewModel<T> : BaseViewModel<T>
{
    public string ConnectionName => _sfmcConnection.Name;
    public string Title => $"Asset Navigator: Connected to {_sfmcConnection.Name}";


    public BaseSfmcViewModel
    (
        ILogger<T> logger,
        SfmcConnection sfmcConnection
    )
        : base
        (
            logger: logger
        )
    {
        _sfmcConnection = sfmcConnection;
    }

    protected readonly SfmcConnection _sfmcConnection;
}

public abstract class BaseSfmcFolderAndListViewModel<T> : BaseSfmcViewModel<T>
{
    public BaseSfmcFolderAndListViewModel
    (
        ILogger<T> logger,
        SfmcConnection sfmcConnection
    )
        : base
        (
            logger: logger,
            sfmcConnection: sfmcConnection
        )
    {
    }

    private string _selectedFolderName = string.Empty;
    public string SelectedFolderName
    {
        get => _selectedFolderName;
        set => SetProperty(ref _selectedFolderName, value);
    }

    private FolderViewModel? _selectedFolder;
    public FolderViewModel? SelectedFolder
    {
        get => _selectedFolder;
        set
        {
            if (SetProperty(ref _selectedFolder, value))
            {
                SelectedFolderName = value?.Name ?? string.Empty;
                LoadAssetForSelectedFolderAsync();
            }
        }
    }

    public abstract Task LoadAssetForSelectedFolderAsync();
}
