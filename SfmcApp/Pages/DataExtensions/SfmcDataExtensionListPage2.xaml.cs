using Microsoft.Extensions.Logging;
using SfmcApp.Models;
using bleak.Martech.SalesforceMarketingCloud.Sfmc.Rest.Assets;
using SfmcApp.ViewModels;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Sfmc.Soap;

#if MACCATALYST
/*using UIKit;
using Foundation;
using CoreGraphics;
using UniformTypeIdentifiers;*/
#endif

namespace SfmcApp.Pages.DataExtensions
{
   public partial class SfmcDataExtensionListPage2 : ContentPage
    {
        private readonly SfmcDataExtensionListViewModel _viewModel;

        public SfmcDataExtensionListPage2(
            SfmcConnection sfmcConnection,
            ILogger<SfmcDataExtensionListViewModel> logger,
            DataExtensionFolderSoapApi folderApi,
            DataExtensionSoapApi objectApi)
        {
            InitializeComponent();

            _viewModel = new SfmcDataExtensionListViewModel(sfmcConnection, logger, folderApi, objectApi);
            BindingContext = _viewModel;

            //SearchBarText.SearchButtonPressed += (s, e) => _viewModel.SearchCommand.Execute(null);
        }
    }
}