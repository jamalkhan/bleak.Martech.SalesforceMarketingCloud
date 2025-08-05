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
        SfmcConnection sfmcConnection,
        ILogger<SfmcDataExtensionListViewModel> logger,
        IDataExtensionFolderApi folderApi,
        IDataExtensionApi objectApi,
        DataExtensionRestApi restApi
    )
    {
        InitializeComponent();

        _viewModel = new SfmcDataExtensionListViewModel(sfmcConnection, logger, folderApi, objectApi, restApi);
        BindingContext = _viewModel;

        //SearchBarText.SearchButtonPressed += (s, e) => _viewModel.SearchCommand.Execute(null);
    }
}
