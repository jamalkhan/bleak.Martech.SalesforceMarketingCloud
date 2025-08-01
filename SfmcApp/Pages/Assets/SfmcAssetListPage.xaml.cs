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
            if (sfmcConnection == null) throw new ArgumentNullException(nameof(sfmcConnection));

            InitializeComponent();
            logger.LogInformation("SfmcAssetListPage initialized with connection: {ConnectionName}", sfmcConnection.Name ?? "Unknown");
            _viewModel = new SfmcAssetListViewModel(sfmcConnection, logger, folderApi, objectApi);
            logger.LogInformation("SfmcAssetListPage initialized with connection: {ConnectionName}", sfmcConnection.Name ?? "Unknown");
            BindingContext = _viewModel;
            logger.LogInformation("SfmcAssetListPage BindingContext set to SfmcAssetListViewModel");
            SearchBarText.SearchButtonPressed += (s, e) => _viewModel.SearchCommand.Execute(null);
        }
        
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (BindingContext is SfmcAssetListViewModel vm)
            {
                await vm.InitializeAsync();
            }
        }

    }
}