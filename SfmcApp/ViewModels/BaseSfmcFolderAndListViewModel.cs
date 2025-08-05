using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using SfmcApp.Models;
using SfmcApp.Models.ViewModels;

namespace SfmcApp.ViewModels;

public abstract class BaseSfmcFolderAndListViewModel
    <
        T,
        TFolderViewModel,
        TFolderApi,
        TContentResourceViewModel,
        TContentResourceApi
    >
    : BaseSfmcViewModel<T>
    where TFolderViewModel : IFolder
{
    public BaseSfmcFolderAndListViewModel
    (
        ILogger<T> logger,
        SfmcConnection sfmcConnection,
        TFolderApi folderApi,
        TContentResourceApi contentResourceApi,
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
        _contentResourceApi = contentResourceApi;
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
                LoadContentResourcesForSelectedFolderAsync();
            }
        }
    }

    private readonly TFolderApi _folderApi;
    public TFolderApi FolderApi => _folderApi;

    public ObservableCollection<TFolderViewModel> Folders { get; } = [];
    #endregion Folders

    #region Search

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    public ObservableCollection<StringSearchOptions> SearchOptions { get; }
            = new ObservableCollection<StringSearchOptions>(
                Enum.GetValues(typeof(StringSearchOptions)).Cast<StringSearchOptions>());

    private StringSearchOptions _selectedSearchOption = StringSearchOptions.Like;
    public StringSearchOptions SelectedSearchOption
    {
        get => _selectedSearchOption;
        set => SetProperty(ref _selectedSearchOption, value);
    }


    #endregion Search

    #region ContentResources

    public ObservableCollection<TContentResourceViewModel> ContentResources { get; } = [];

    private readonly TContentResourceApi _contentResourceApi;
    public TContentResourceApi ContentResourceApi => _contentResourceApi;
    private bool _isContentResourcesLoading;
    public bool IsContentResourcesLoading
    {
        get => _isContentResourcesLoading;
        set => SetProperty(ref _isContentResourcesLoading, value);
    }

    private bool _isContentResourcesLoaded;
    public bool IsContentResourcesLoaded
    {
        get => _isContentResourcesLoaded;
        set => SetProperty(ref _isContentResourcesLoaded, value);
    }

    #endregion ContentResources

    public abstract Task LoadContentResourcesForSelectedFolderAsync();

    public abstract Task LoadFoldersAsync();
}
