using System.Collections.ObjectModel;
using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.Authentication;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Sfmc.Rest.DataExtensions;
using bleak.Martech.SalesforceMarketingCloud.Models;
using bleak.Martech.SalesforceMarketingCloud.Configuration;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Sfmc.Soap;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using bleak.Martech.SalesforceMarketingCloud.Rest;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Fileops;
using bleak.Martech.SalesforceMarketingCloud.Wsdl;
using SfmcApp.Models;
using SfmcApp.Pages.BasePages;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Sfmc.Rest.Content;
using bleak.Martech.SalesforceMarketingCloud.Models.SfmcDtos;
using Microsoft.Extensions.Logging;







#if MACCATALYST
using UIKit;
using Foundation;
using CoreGraphics;
using UniformTypeIdentifiers;
#endif





namespace SfmcApp.Pages.Content;

public partial class SfmcContentListPage : ContentPage, INotifyPropertyChanged
{
    private readonly IContentFolderRestApi _folderApi;
    private readonly ILogger<SfmcContentListPage> _logger;

    public new event PropertyChangedEventHandler PropertyChanged;

    private new void OnPropertyChanged([CallerMemberName] string name = "") =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));


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



    
    public ObservableCollection<FolderObject> Folders { get; set; } = new();
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
                LoadContentForSelectedFolderAsync();
            }
        }
    }

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
    #endregion Bound Properties


    private readonly IAuthRepository _authRepository;


    public SfmcContentListPage(
        IAuthRepository authRepository,
        ILogger<SfmcContentListPage> logger,
        IContentFolderRestApi folderApi
        )
    {
        InitializeComponent();
        BindingContext = this;
        /*
        


        SearchBarText.SearchButtonPressed += (s, e) =>
        {
            OnSearchButtonClicked(s, e);
        };
        _folderApi = folderApi ?? throw new ArgumentNullException(nameof(folderApi));
        // Safely load folders in the background
        LoadFoldersAsync();
        */
    }
    private async void LoadFoldersAsync()
    {
        try
        {
            IsFoldersLoaded = false;
            IsFoldersLoading = true;
            _logger.LogInformation($"Preparing to Load folders from API");
            var folderTree = await _folderApi.GetFolderTreeAsync(); // Must be async method
            _logger.LogInformation($"Loaded {folderTree.Count} folders from API");
            foreach (FolderObject folder in folderTree)
            {
                Folders.Add(folder);
            }
            IsFoldersLoaded = true;
            IsFoldersLoading = false;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load folders: {ex.ToString()}", "OK");
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


    private void LoadContentForSelectedFolderAsync()
    {
        throw new NotImplementedException();
    }

    private async void OnDownloadTapped(object sender, EventArgs e)
    {
        throw new NotImplementedException();
    }
}