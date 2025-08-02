using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using SfmcApp.Models;
using SfmcApp.Models.ViewModels;

namespace SfmcApp.ViewModels;

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
