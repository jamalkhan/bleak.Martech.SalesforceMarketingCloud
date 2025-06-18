using bleak.Martech.SalesforceMarketingCloud.Rest;
using bleak.Martech.SalesforceMarketingCloud.Fileops;
using bleak.Martech.SalesforceMarketingCloud.Authentication;
using bleak.Martech.SalesforceMarketingCloud.Configuration;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Sfmc.Soap;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using SfmcApp.Models;
using bleak.Martech.SalesforceMarketingCloud.Models;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Sfmc.Rest.Assets;
using bleak.Martech.SalesforceMarketingCloud.Models.Pocos;

#if MACCATALYST
/*using UIKit;
using Foundation;
using CoreGraphics;
using UniformTypeIdentifiers;*/
#endif

namespace SfmcApp.Pages.Assets
{
    public partial class SfmcAssetListPage : ContentPage, INotifyPropertyChanged
    {
        #region Move To Base?
        private readonly ILogger<SfmcAssetListPage> _logger;
        public new event PropertyChangedEventHandler PropertyChanged;

        private new void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion Move To Base?
        

        #region Folders
        private bool _isFoldersLoading;
        public bool IsFoldersLoading
        {
            get => _isFoldersLoading;
            set
            {
                _isFoldersLoading = value;
                OnPropertyChanged(); // or SetProperty in CommunityToolkit
            }
        }
        private bool _isFoldersLoaded;
        public bool IsFoldersLoaded
        {
            get => _isFoldersLoaded;
            set
            {
                _isFoldersLoaded = value;
                OnPropertyChanged(); // or SetProperty in CommunityToolkit
            }
        }
        
        public ObservableCollection<FolderObject> Folders { get; set; } = new();
        #endregion Folders

        private readonly IAssetFolderRestApi _folderApi;
        private readonly IAssetRestApi _objectApi;

        #region Constructor
        public SfmcAssetListPage(
            ILogger<SfmcAssetListPage> logger,
            IAssetFolderRestApi folderApi
            )
        {
            InitializeComponent();
            BindingContext = this;
            SearchBarText.SearchButtonPressed += (s, e) =>
            {
                OnSearchButtonClicked(s, e);
            };
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _folderApi = folderApi ?? throw new ArgumentNullException(nameof(folderApi));
            _logger.LogInformation("SfmcAssetListPage initialized with API and logger");
            // Safely load folders in the background
            LoadFoldersAsync();
        }
        #endregion Constructor

        private async void LoadFoldersAsync()
        {
            try
            {
                IsFoldersLoaded = false;
                IsFoldersLoading = true;
                var folderTree = await _folderApi.GetFolderTreeAsync(); // Must be async method
                _logger.LogInformation($"Loaded {folderTree.Count} Asset Folders from API");
                foreach (FolderObject folder in folderTree)
                {
                    Folders.Add(folder);
                }
                IsFoldersLoaded = true;
                IsFoldersLoading = false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to load Asset Folders: {ex.ToString()}");
            }
        }


        #region Search Functionality
        public ObservableCollection<StringSearchOptions> SearchOptions { get; }
            = new ObservableCollection<StringSearchOptions>(
                Enum.GetValues(typeof(StringSearchOptions)).Cast<StringSearchOptions>());

        private StringSearchOptions _selectedSearchOption = StringSearchOptions.Like;
        public StringSearchOptions SelectedSearchOption
        {
            get => _selectedSearchOption;
            set
            {
                if (_selectedSearchOption != value)
                {
                    _selectedSearchOption = value;
                    OnPropertyChanged();
                }
            }
        }
        private async void OnSearchButtonClicked(object sender, EventArgs e)
        {
            throw new NotImplementedException();
            /*
            var api = new DataExtensionSoapApi(
                authRepository: _authRepository,
                config: new SfmcConnectionConfiguration()
                );

            DataExtensions.Clear();
            string searchType = SearchTypePicker.SelectedItem?.ToString() ?? "Like";
            string searchText = SearchBarText.Text?.Trim() ?? string.Empty;

            // Check if search text is empty
            if (string.IsNullOrEmpty(searchText))
            {
                await DisplayAlert("Error", "Please enter a search term.", "OK");
                return;
            }

            try
            {
                // Assume DataExtensions is a property in the view model or code-behind
                // and api is an instance of your API client
                List<DataExtensionPoco> results = new List<DataExtensionPoco>();
                switch (searchType)
                {
                    case "Starts With":
                        results = await api.GetDataExtensionsNameStartsWithAsync(searchText);

                        break;
                    case "Like":
                        results = await api.GetDataExtensionsNameLikeAsync(searchText);
                        break;
                    case "Ends With":
                        results = await api.GetDataExtensionsNameEndsWithAsync(searchText);
                        break;
                    default:
                        results = await api.GetDataExtensionsNameLikeAsync(searchText);
                        break;
                }
                foreach (var item in results)
                {
                    DataExtensions.Add(item);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to perform search: {ex.Message}", "OK");
            }*/

            // You can access the SearchBar and Picker values if they are named, e.g.:
            // var searchText = SearchBarName.Text; // If SearchBar has x:Name="SearchBarName"
            // var searchType = PickerName.SelectedItem?.ToString(); // If Picker has x:Name="PickerName"
        }
        #endregion Search Functionality

        #region Folder Selection
        public ObservableCollection<AssetPoco> Assets { get; set; } = new();
        private DataExtensionFolder _selectedFolder;
        public DataExtensionFolder SelectedFolder
        {
            get => _selectedFolder;
            set
            {
                if (_selectedFolder != value)
                {
                    _selectedFolder = value;
                    OnPropertyChanged();
                    LoadAssetForSelectedFolderAsync();
                }
            }
        }

        public ICommand FolderTappedCommand => new Command<DataExtensionFolder>(folder =>
        {
            SelectedFolder = folder;
        });


        private async void LoadAssetForSelectedFolderAsync()
        {
             try
            {
                Assets.Clear();

                if (_selectedFolder == null)
                    return;

                // Replace with actual logic to fetch Data Extensions for the selected folder
                var assets = await _objectApi.GetAssetsAsync(_selectedFolder.Id);

                foreach (var asset in assets)
                {
                    Assets.Add(asset);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to load Assets for folder {_selectedFolder.Name}: {ex.ToString()}");
            }
        }
#endregion Folder Selection

        private async void OnDownloadTapped(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}