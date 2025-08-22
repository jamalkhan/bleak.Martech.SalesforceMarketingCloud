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


public partial class SfmcDataExtensionFileImportPage : ContentPage
{
    private readonly SfmcDataExtensionFileImportViewModel _viewModel;

    public SfmcDataExtensionFileImportPage
    (
        SfmcDataExtensionFileImportViewModel viewModel,
        INavigationService navigationService
    )
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;

        //SearchBarText.SearchButtonPressed += (s, e) => _viewModel.SearchCommand.Execute(null);
    }

}