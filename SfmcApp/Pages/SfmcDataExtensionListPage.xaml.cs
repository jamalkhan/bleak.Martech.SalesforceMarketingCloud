using System.Collections.ObjectModel;
using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.Authentication;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Sfmc.Rest.DataExtensions;
using bleak.Martech.SalesforceMarketingCloud.Models;
using bleak.Martech.SalesforceMarketingCloud.Configuration;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Sfmc.Soap;
namespace SfmcApp.Pages;

public partial class SfmcDataExtensionListPage : ContentPage
{
	public AuthRepository _authRepository { get; private set; }
    private readonly DataExtensionFolderSoapApi _api;
	public ObservableCollection<DataExtensionFolder> Folders { get; set; } = new();
	static JsonSerializer _serializer = new JsonSerializer();
    static RestManager _restManager = new RestManager(_serializer, _serializer);

	public SfmcDataExtensionListPage(AuthRepository authRepository)
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
}