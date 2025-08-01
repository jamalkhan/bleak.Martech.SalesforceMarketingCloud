using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.Sfmc.Rest.Assets;
using bleak.Martech.SalesforceMarketingCloud.Sfmc.Rest.DataExtensions;
using Microsoft.Extensions.Logging;
using SfmcApp.Models;
using SfmcApp.Models.ViewModels;

namespace SfmcApp.ViewModels
{
    /*
    public class SfmcDataExtensionListViewModel : INotifyPropertyChanged
    {
        private readonly ILogger<SfmcDataExtensionListViewModel> _logger;
        private readonly IDataExtensionFolderRestApi _folderApi;
        private readonly IDataExtensionRestApi _dataExtensionApi;
        private readonly SfmcConnection _sfmcConnection;

        public ObservableCollection<FolderViewModel> Folders { get; } = new();
        public ObservableCollection<DataExtensionViewModel> DataExtensions { get; } = new();


        public SfmcDataExtensionListViewModel(
            ILogger<SfmcDataExtensionListViewModel> logger,
            IDataExtensionFolderRestApi folderApi,
            IDataExtensionRestApi objectApi)
        {
            _logger = logger;
            _folderApi = folderApi;
            _objectApi = objectApi;
        }

        #region Common Properties
        public event PropertyChangedEventHandler? PropertyChanged;
        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }
        #endregion Common Properties
    }
    */
}