using bleak.Martech.SalesforceMarketingCloud.Rest;
using bleak.Martech.SalesforceMarketingCloud.Fileops;
using bleak.Martech.SalesforceMarketingCloud.Authentication;
using bleak.Martech.SalesforceMarketingCloud.Configuration;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Sfmc.Soap;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using SfmcApp.Models;
using bleak.Martech.SalesforceMarketingCloud.Models;
using bleak.Martech.SalesforceMarketingCloud.Sfmc.Rest.Assets;
using bleak.Martech.SalesforceMarketingCloud.Models.Pocos;
using SfmcApp.Models.ViewModels;
using System.Text.Json.Serialization;
using bleak.Api.Rest;

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