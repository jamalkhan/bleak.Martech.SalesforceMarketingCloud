using System.Collections.ObjectModel;
using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.Authentication;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Sfmc.Rest.DataExtensions;
using bleak.Martech.SalesforceMarketingCloud.Models;
using bleak.Martech.SalesforceMarketingCloud.Configuration;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Sfmc.Soap;
using System.ComponentModel;
using System.Runtime.CompilerServices;
namespace SfmcApp.Pages;

public partial class SfmcDataExtensionListPage : ContentPage, INotifyPropertyChanged
{
	public IAuthRepository _authRepository { get; private set; }
    private readonly DataExtensionFolderSoapApi _api;
	public ObservableCollection<DataExtensionFolder> Folders { get; set; } = new();
    public ObservableCollection<DataExtensionPoco> DataExtensions { get; set; } = new();

    public event PropertyChangedEventHandler PropertyChanged;

    void OnPropertyChanged([CallerMemberName] string name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

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

    static JsonSerializer _serializer = new JsonSerializer();
    static RestManager _restManager = new RestManager(_serializer, _serializer);

	public SfmcDataExtensionListPage(IAuthRepository authRepository)
	{
		InitializeComponent();
        _authRepository = authRepository;

        _api = new DataExtensionFolderSoapApi(
			authRepository: _authRepository,
			config: new SfmcConnectionConfiguration()
			);

        BindingContext = this;

        // Safely load folders in the background
        LoadFoldersAsync();
	}

	private async void LoadFoldersAsync()
    {
        try
        {
            var folderTree = await _api.GetFolderTreeAsync(); // Must be async method
            foreach (var folder in folderTree)
            {
                Folders.Add(folder);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load folders: {ex.Message}", "OK");
        }
    }


    private async void LoadDataExtensionsForSelectedFolderAsync()
    {
        try
        {
            var api = new DataExtensionSoapApi(
                authRepository:_authRepository,
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
}