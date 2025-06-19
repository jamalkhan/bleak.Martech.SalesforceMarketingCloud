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
using bleak.Martech.SalesforceMarketingCloud.Sfmc.Rest.Assets;
using bleak.Martech.SalesforceMarketingCloud.Models.Pocos;
using SfmcApp.Models.ViewModels;
using System.Text.Json.Serialization;
using bleak.Api.Rest;

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
        public new event PropertyChangedEventHandler? PropertyChanged;

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
            IAssetFolderRestApi folderApi,
            IAssetRestApi objectApi
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
            _objectApi = objectApi ?? throw new ArgumentNullException(nameof(objectApi));
            _logger.LogInformation("SfmcAssetListPage initialized with API and logger");
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
        public ObservableCollection<AssetViewModel> Assets { get; set; } = new();
        private FolderObject? _selectedFolder;
        public FolderObject? SelectedFolder
        {
            get => _selectedFolder;
            set
            {
                _logger.LogInformation($"Changing SelectedFolder to: {value?.Name ?? "None"}");
                if (_selectedFolder != value)
                {
                    _selectedFolder = value;
                    _logger.LogInformation($"SelectedFolder changed to: {value?.Name ?? "None"}");
                    OnPropertyChanged();
                    _logger.LogInformation($"Calling LoadAssetForSelectedFolderAsync for folder: {_selectedFolder?.Name ?? "None"}");
                    LoadAssetForSelectedFolderAsync();
                }
            }
        }

        public ICommand FolderTappedCommand => new Command<FolderObject>(folder =>
        {
            SelectedFolder = folder;
        });


        private async void LoadAssetForSelectedFolderAsync()
        {
            try
            {
                _logger.LogInformation($"Loading Assets for folder: {_selectedFolder?.Name ?? "None"}");
                Assets.Clear();

                if (_selectedFolder == null)
                    return;

                // Replace with actual logic to fetch Data Extensions for the selected folder
                var assets = await _objectApi.GetAssetsAsync(_selectedFolder.Id);
                _logger.LogInformation($"Loaded {assets.Count} Assets for folder {_selectedFolder.Name}");

                foreach (var asset in assets.ToViewModelList())
                {
                    Assets.Add(asset);
                }
                _logger.LogInformation($"Assets loaded for folder {_selectedFolder.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to load Assets for folder {_selectedFolder?.Name ?? "None"}: {ex.ToString()}");
            }
        }
        #endregion Folder Selection

        // TODO: Make this a configuration option
        bool usePublishedUrlName = true;

        
        private async void OnDownloadTapped(object sender, EventArgs e)
        {

            if (sender is Image image && image.BindingContext is AssetViewModel asset)
            {
                if (asset.IsBinaryDownloadable)
                {
                    await DownloadBinaryAsync(asset);
                }
                else
                {
                    await DownloadTextAsync(asset);
                }
            }
        }
        private string GetFileName(AssetViewModel asset)
        {
            if (usePublishedUrlName)
            {
                string url = asset.FileProperties.PublishedURL;
                Uri uri = new Uri(url);
                string fileName = Path.GetFileName(uri.LocalPath);
                return fileName;
            }
            return asset.FileProperties.FileName;
        }
        private string GetTextFileName(AssetViewModel asset)
        {
            return $"{asset.Name}-{asset.CustomerKey}.{asset.AssetType.Name}.ampscript";
        }
        private async Task DownloadTextAsync(AssetViewModel asset)
        {
            var serializer = new JsonSerializer();
            var fileName = GetTextFileName(asset);
            var metadataFile = Path.Combine(FileSystem.AppDataDirectory, "Downloads", "Assets", $"{fileName}.metadata.json");
            Directory.CreateDirectory(Path.GetDirectoryName(metadataFile)!);
            // Write Metadata to Named as Customer Key
            //var metaDataCustomerKey = AppConfiguration.Instance.OutputFolder + "/" + asset.FullPath + "/customerkey-" + asset.CustomerKey + ".metadata";

            _logger.LogTrace($"Writing {fileName} metadata file to {metadataFile}");
            await File.WriteAllTextAsync(metadataFile, serializer.Serialize(asset));
            string outputFileName = Path.Combine(FileSystem.AppDataDirectory, "Downloads", "Assets", fileName);
            _logger.LogTrace($"Writing {fileName} {asset.AssetType.Name} content to {outputFileName}");
            await File.WriteAllTextAsync(outputFileName, asset.Content);
            _logger.LogInformation($"File Written to File System: {outputFileName}");
        }

        private async Task DownloadBinaryAsync(AssetViewModel asset)
        {
            JsonSerializer serializer = new JsonSerializer();
            string fileName = GetFileName(asset);
            
            // TODO: Make the file path include the connection name or some identifier
            var metadataFile = Path.Combine(FileSystem.AppDataDirectory, "Downloads", "Assets", $"{fileName}.metadata.json");
        
            Directory.CreateDirectory(Path.GetDirectoryName(metadataFile)!);
            _logger.LogTrace($"Trying to save {metadataFile}");

            File.WriteAllText(metadataFile, serializer.Serialize(asset));

            var imageUrl = asset.FileProperties.PublishedURL;
            using (var client = new HttpClient())
            {
                try
                {
                    var imageFilePath = Path.Combine(FileSystem.AppDataDirectory, "Downloads", "Assets", fileName);
                    var imageBytes = client.GetByteArrayAsync(imageUrl).Result;
                    Directory.CreateDirectory(Path.GetDirectoryName(imageFilePath)!);
                    await File.WriteAllBytesAsync(imageFilePath, imageBytes);
                    _logger.LogInformation($"Image saved to {imageFilePath}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to download image from {imageUrl}: {ex.Message}");
                }
            }
        }
    }
}