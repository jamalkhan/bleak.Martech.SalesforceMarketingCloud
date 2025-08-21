using Microsoft.Extensions.Logging;
using SfmcApp.Models;
using SfmcApp.ViewModels;
using bleak.Martech.SalesforceMarketingCloud.Api;
using bleak.Martech.SalesforceMarketingCloud.Sfmc.Rest.DataExtensions;

#if MACCATALYST
/*using UIKit;
using Foundation;
using CoreGraphics;
using UniformTypeIdentifiers;*/
#endif

namespace SfmcApp.Pages.DataExtensions;

public partial class SfmcDataExtensionListPage2 : ContentPage
{
    private readonly SfmcDataExtensionListViewModel _viewModel;

    public SfmcDataExtensionListPage2
    (
        SfmcDataExtensionListViewModel viewModel
    )
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;

        SearchBarText.SearchButtonPressed += (s, e) => _viewModel.SearchCommand.Execute(null);
    }

    protected async override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        try
        {
            await _viewModel.LoadFoldersAsync();
        }
        catch (TimeoutException ex)
        {
            // Log the exception and show a user-friendly message
            System.Diagnostics.Debug.WriteLine($"Timeout loading folders: {ex.Message}");
            await DisplayAlert("Connection Timeout", ex.Message, "OK");
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Authentication failed"))
        {
            // Log the exception and show a user-friendly message
            System.Diagnostics.Debug.WriteLine($"Authentication error: {ex.Message}");
            await DisplayAlert("Authentication Error", "Failed to authenticate with Salesforce Marketing Cloud. Please check your connection settings.", "OK");
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Network connectivity test failed"))
        {
            // Log the exception and show a user-friendly message
            System.Diagnostics.Debug.WriteLine($"Network connectivity error: {ex.Message}");
            await DisplayAlert("Network Error", "Network connectivity test failed. Please check your internet connection and try again.", "OK");
        }
        catch (Exception ex)
        {
            // Log the exception and show a user-friendly message
            System.Diagnostics.Debug.WriteLine($"Error loading folders: {ex}");
            await DisplayAlert("Error", $"Failed to load folders: {ex.Message}", "OK");
        }
    }
}
