using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using bleak.Martech.SalesforceMarketingCloud.Sfmc.Rest.Assets;
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

    private string _downloadDirectory;
    public string DownloadDirectory
    {
        get => _downloadDirectory;
        set => SetProperty(ref _downloadDirectory, value);
    }

    public BaseSfmcViewModel
    (
        ILogger<T> logger,
        SfmcConnection sfmcConnection,
        string resourceType
    )
        : base
        (
            logger: logger
        )
    {
        _downloadDirectory = Path.Combine(FileSystem.AppDataDirectory, "Downloads", sfmcConnection.DirectoryName, resourceType);
        _sfmcConnection = sfmcConnection;
    }

    protected readonly SfmcConnection _sfmcConnection;
}

public abstract class BaseSfmcFolderAndListViewModel
    <T, TFolderViewModel, TFolderApi, TAssetViewModel, TAssetApi>
    : BaseSfmcViewModel<T>
    where TFolderViewModel : IFolder
{
    public BaseSfmcFolderAndListViewModel
    (
        ILogger<T> logger,
        SfmcConnection sfmcConnection,
        TFolderApi folderApi,
        TAssetApi assetApi,
        string resourceType
    )
        : base
        (
            logger: logger,
            sfmcConnection: sfmcConnection,
            resourceType: resourceType
        )
    {
        _folderApi = folderApi;
        _assetApi = assetApi;
    }

    #region Folders
    private bool _isFoldersLoading;
    public bool IsFoldersLoading
    {
        get => _isFoldersLoading;
        set => SetProperty(ref _isFoldersLoading, value);
    }

    private bool _isFoldersLoaded;
    public bool IsFoldersLoaded
    {
        get => _isFoldersLoaded;
        set => SetProperty(ref _isFoldersLoaded, value);
    }

    private string _selectedFolderName = string.Empty;
    public string SelectedFolderName
    {
        get => _selectedFolderName;
        set => SetProperty(ref _selectedFolderName, value);
    }

    private TFolderViewModel? _selectedFolder;
    public TFolderViewModel? SelectedFolder
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

    private readonly TFolderApi _folderApi;
    public TFolderApi FolderApi => _folderApi;

    public ObservableCollection<TFolderViewModel> Folders { get; } = [];
    #endregion Folders

    #region Assets

    public ObservableCollection<TAssetViewModel> Assets { get; } = [];

    private readonly TAssetApi _assetApi;
    public TAssetApi AssetApi => _assetApi; 
    private bool _isAssetsLoading;
    public bool IsAssetsLoading
    {
        get => _isAssetsLoading;
        set => SetProperty(ref _isAssetsLoading, value);
    }

    private bool _isAssetsLoaded;
    public bool IsAssetsLoaded
    {
        get => _isAssetsLoaded;
        set => SetProperty(ref _isAssetsLoaded, value);
    }

    #endregion Assets

    public abstract Task LoadAssetForSelectedFolderAsync();

    public abstract Task LoadFoldersAsync();
}
