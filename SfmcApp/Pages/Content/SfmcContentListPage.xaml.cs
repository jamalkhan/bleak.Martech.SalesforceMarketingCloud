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




#if MACCATALYST
using UIKit;
using Foundation;
using CoreGraphics;
using UniformTypeIdentifiers;
#endif



namespace SfmcApp.Pages.Content;

public partial class SfmcContentListPage : BasePage
{
    public new event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));


    public SfmcContentListPage(IAuthRepository authRepository)
        : base(authRepository: authRepository)
    {
        InitializeComponent();
        BindingContext = this;

        SearchBarText.SearchButtonPressed += (s, e) =>
        {
            OnSearchButtonClicked(s, e);
        };
        // Safely load folders in the background
        LoadFoldersAsync();
    }
    private async void LoadFoldersAsync()
    {
    }

    private async void OnSearchButtonClicked(object sender, EventArgs e)
    {
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

    private async void OnDownloadTapped(object sender, EventArgs e)
    {
    }
}