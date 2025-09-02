using Microsoft.Extensions.Logging;
using SfmcApp.Models;
using SfmcApp.ViewModels;
using bleak.Martech.SalesforceMarketingCloud.Api;
using bleak.Martech.SalesforceMarketingCloud.Sfmc.Rest.DataExtensions;
using Microsoft.Maui.Controls;
using System;
using System.Linq;
using SfmcApp.ViewModels.Services;


#if MACCATALYST
using UIKit;
using Foundation;
using CoreGraphics;
using UniformTypeIdentifiers;
#elif WINDOWS
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
#elif IOS
using UIKit;
using Foundation;
#endif


namespace SfmcApp.Pages.DataExtensions;

public partial class SfmcDataExtensionListPage : ContentPage
{
    private readonly SfmcDataExtensionListViewModel _viewModel;

    public SfmcDataExtensionListPage
    (
        SfmcDataExtensionListViewModel viewModel,
        INavigationService navigationService
    )
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;

        SearchBarText.SearchButtonPressed += (s, e) => _viewModel.SearchCommand.Execute(null);
    }

    private void OnDragOver(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = DataPackageOperation.Copy;
    }

    private async void OnFileDrop(object sender, DropEventArgs e)
    {
        var files = new List<string>();

#if WINDOWS
        var args = e.PlatformArgs?.DragEventArgs;
        if (args != null && args.DataView.Contains(StandardDataFormats.StorageItems))
        {
            var items = await args.DataView.GetStorageItemsAsync();
            foreach (var item in items)
            {
                if (item is StorageFile f)
                {
                    var ext = Path.GetExtension(f.Path)?.ToLowerInvariant();
                    if (ext == ".csv" || ext == ".tsv")
                        files.Add(f.Path);
                }
            }
        }
#elif IOS || MACCATALYST
        var session = e.PlatformArgs?.DropSession;
        if (session != null)
        {
            foreach (UIDragItem item in session.Items)
            {
                // Walk registered type identifiers and get a file URL
                var ids = item.ItemProvider.RegisteredTypeIdentifiers.ToList();
                var result = await LoadItemAsync(item.ItemProvider, ids);
                var path = result?.FileUrl?.Path;
                var ext = Path.GetExtension(path ?? "")?.ToLowerInvariant();
                if (!string.IsNullOrEmpty(path) && (ext == ".csv" || ext == ".tsv"))
                    files.Add(path);
            }

            static async Task<LoadInPlaceResult?> LoadItemAsync(NSItemProvider provider, List<string> ids)
            {
                if (ids.Count == 0) return null;
                var id = ids[0];
                if (provider.HasItemConformingTo(id))
                    return await provider.LoadInPlaceFileRepresentationAsync(id);
                ids.RemoveAt(0);
                return await LoadItemAsync(provider, ids);
            }
        }
#endif

#if ANDROID
        // Dragging from other apps to MAUI is not supported on Android.
#endif

        if (files.Count > 0)
        {
            // Open your import modal (replace with your own page/service/VM command)
            /* await Shell.Current.Navigation.PushModalAsync(new ImportCsvTsvPage(files)); */
            // or ((SfmcDataExtensionListViewModel)BindingContext).OpenImportModalCommand.Execute(files);
            ((SfmcDataExtensionListViewModel)BindingContext).FileDroppedCommand.Execute(files);
        }
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
