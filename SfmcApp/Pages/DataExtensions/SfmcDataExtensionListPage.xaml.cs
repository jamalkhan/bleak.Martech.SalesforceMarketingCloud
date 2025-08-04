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
using bleak.Martech.SalesforceMarketingCloud.Sfmc.Rest.DataExtensions;
using bleak.Martech.SalesforceMarketingCloud.Api.Soap;

#if MACCATALYST
/*
using UIKit;
using Foundation;
using CoreGraphics;
using UniformTypeIdentifiers;
*/
#endif

namespace SfmcApp.Pages
{
    /*public abstract class BaseListPage<T> : ContentPage
    {
        public new event PropertyChangedEventHandler PropertyChanged;
        public SfmcDataExtensionListPage()
        {
            InitializeComponent();
        }
    }*/
}

namespace SfmcApp.Pages.DataExtensions
{
    public partial class SfmcDataExtensionListPage
    : ContentPage
    , INotifyPropertyChanged
    {
        private readonly ILogger<SfmcDataExtensionListPage> _logger;
        public new event PropertyChangedEventHandler PropertyChanged;

        private new void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


        #region Bound Properties
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

        public ObservableCollection<DataExtensionFolder> Folders { get; set; } = new();
        public ObservableCollection<DataExtensionPoco> DataExtensions { get; set; } = new();
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
                    LoadDataExtensionsForSelectedFolderAsync();
                }
            }
        }

        #endregion Bound Properties

        private readonly IAuthRepository _authRepository;

        public SfmcDataExtensionListPage(
            //ILogger<SfmcDataExtensionListPage> logger, IContentFolderRestApi api
            IAuthRepository authRepository
            )
        {
            InitializeComponent();
            BindingContext = this;
            SearchBarText.SearchButtonPressed += (s, e) =>
            {
                OnSearchButtonClicked(s, e);
            };
            //_logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _authRepository = authRepository;
            //_logger.LogInformation("SfmcDataExtensionListPage initialized with API and logger");
            // Safely load folders in the background
            LoadFoldersAsync();
        }

        private async void LoadFoldersAsync()
        {
            try
            {
                IsFoldersLoaded = false;
                IsFoldersLoading = true;
                var folderApi = new DataExtensionFolderSoapApi(
                    authRepository: _authRepository,
                    config: new SfmcConnectionConfiguration()
                    );
                //_logger.LogInformation($"Loaded {folderTree.Count} Data Extension Folders from API");
                var folderTree = await folderApi.GetFolderTreeAsync(); // Must be async method
                foreach (var folder in folderTree)
                {
                    Folders.Add(folder);
                }
                IsFoldersLoaded = true;
                IsFoldersLoading = false;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load Data Extension Folders: {ex.Message}", "OK");
                //_logger.LogError($"Failed to load Data Extension Folders: {ex.ToString()}");
            }
        }


        #region Folder Selection
        public ICommand FolderTappedCommand => new Command<DataExtensionFolder>(folder =>
        {
            SelectedFolder = folder;
        });


        private async void LoadDataExtensionsForSelectedFolderAsync()
        {
            try
            {
                var api = new DataExtensionSoapApi(
                    authRepository: _authRepository,
                    config: new SfmcConnectionConfiguration()
                    );

                DataExtensions.Clear();

                if (_selectedFolder == null)
                    return;

                // Replace with actual logic to fetch Data Extensions for the selected folder
                var dataExtensions = await api.GetDataExtensionsByFolderAsync(_selectedFolder.Id);

                foreach (var de in dataExtensions)
                {
                    DataExtensions.Add(de);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load data extensions: {ex.Message}", "OK");
            }
        }
        #endregion Folder Selection

        private async void OnDownloadCsvTapped(object sender, EventArgs e)
        {
            if (sender is Image image && image.BindingContext is DataExtensionPoco dataExtension)
            {
                var filePath = Path.Combine(FileSystem.AppDataDirectory, $"{dataExtension.Name}.csv");

                try
                {
                    // Optional: Show loading UI
                    await DisplayAlert("Downloading", $"Starting download of {dataExtension.Name} to {filePath}...", "OK");

                    var api = new DataExtensionRestApi(_authRepository);
                    IFileWriter fileWriter = new DelimitedFileWriter(
                        new DelimitedFileWriterOptions { Delimiter = "," });

                    // Run long sync task in background
                    long records = await Task.Run(() =>
                        api.DownloadDataExtension(
                            dataExtensionCustomerKey: dataExtension.CustomerKey,
                            fileWriter: fileWriter,
                            fileName: filePath
                        )
                    );

                    await DisplayAlert("Success", $"Downloaded {records} records to {filePath}", "OK");
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"Download failed: {ex.ToString()}", "OK");
                }
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
            }

            // You can access the SearchBar and Picker values if they are named, e.g.:
            // var searchText = SearchBarName.Text; // If SearchBar has x:Name="SearchBarName"
            // var searchType = PickerName.SelectedItem?.ToString(); // If Picker has x:Name="PickerName"
        }
        #endregion Search Functionality

        /*private async Task<string?> PromptUserForSaveLocationAsync(string suggestedFileName)
        {
        #if MACCATALYST
            var panel = new UIKit.UIDocumentPickerViewController(
                new[] { UTType.CommaSeparatedText },
                UIDocumentPickerMode.ExportToService);

            string? selectedPath = null;

            var tcs = new TaskCompletionSource<string?>();

            panel.DidPickDocument += (sender, args) =>
            {
                var url = args.Url;
                if (url != null)
                {
                    selectedPath = url.Path;
                }
                tcs.SetResult(selectedPath);
            };

            panel.WasCancelled += (sender, e) => tcs.SetResult(null);

            var windowScene = UIApplication.SharedApplication.ConnectedScenes
                .OfType<UIWindowScene>().FirstOrDefault();

            var window = windowScene?.Windows.FirstOrDefault();
            if (window?.RootViewController != null)
            {
                window.RootViewController.PresentViewController(panel, true, null);
            }

            return await tcs.Task;
        #else
            // Fallback or throw for other platforms
            await Application.Current.MainPage.DisplayAlert("Unsupported", "File picker only supported on macOS in this build.", "OK");
            return null;
        #endif
        }
    */
    }
}