using Microsoft.Extensions.Logging;
using SfmcApp.Models;
using bleak.Martech.SalesforceMarketingCloud.Sfmc.Rest.Assets;
using SfmcApp.ViewModels;

#if MACCATALYST
/*using UIKit;
using Foundation;
using CoreGraphics;
using UniformTypeIdentifiers;*/
#endif

namespace SfmcApp.Pages.Assets
{
   public partial class SfmcAssetListPage : ContentPage
    {
        private readonly SfmcAssetListViewModel _viewModel;

        public SfmcAssetListPage(
            SfmcConnection sfmcConnection,
            ILogger<SfmcAssetListViewModel> logger,
            IAssetFolderRestApi folderApi,
            IAssetRestApi objectApi)
        {
            InitializeComponent();

            _viewModel = new SfmcAssetListViewModel(sfmcConnection, logger, folderApi, objectApi);
            BindingContext = _viewModel;

            SearchBarText.SearchButtonPressed += (s, e) => _viewModel.SearchCommand.Execute(null);
        }
    }
}