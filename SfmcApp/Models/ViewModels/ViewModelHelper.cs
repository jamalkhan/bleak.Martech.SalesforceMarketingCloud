using bleak.Martech.SalesforceMarketingCloud;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Sfmc.Soap;
using bleak.Martech.SalesforceMarketingCloud.Models;
using bleak.Martech.SalesforceMarketingCloud.Models.Pocos;



namespace SfmcApp.Models.ViewModels
{
    public static class ViewModelHelper
    {


        /// <summary>
        /// Converts a list of AssetPoco to a list of AssetViewModel.
        /// </summary>
        /// <param name="assets">List of AssetPoco.</param>
        /// <returns>List of AssetViewModel.</returns>
        public static List<AssetViewModel> ToViewModel(this List<AssetPoco> assets)
        {
            return assets.Select(
                asset => asset.ToViewModel()
                ).ToList();
        }

        public static AssetViewModel ToViewModel(this AssetPoco asset)
        {
            return new AssetViewModel
            {
                Id = asset.Id,
                CustomerKey = asset.CustomerKey,
                ObjectID = asset.ObjectID,
                AssetType = asset.AssetType,
                Name = asset.Name,
                Description = asset.Description,
                CreatedDate = asset.CreatedDate,
                CreatedBy = asset.CreatedBy,
                ModifiedDate = asset.ModifiedDate,
                ModifiedBy = asset.ModifiedBy,
                EnterpriseId = asset.EnterpriseId,
                MemberId = asset.MemberId,
                Status = asset.Status,
                Thumbnail = asset.Thumbnail,
                Category = asset.Category,
                Content = asset.Content,
                Data = asset.Data,
                FileProperties = asset.FileProperties,
                Views = asset.Views,
                FullPath = asset.FullPath
            };
        }
        public static FolderViewModel ToViewModel(this FolderObject folder)
        {
            return new FolderViewModel
            {
                Id = folder.Id,
                Description = folder.Description,
                EnterpriseId = folder.EnterpriseId,
                MemberId = folder.MemberId,
                Name = folder.Name,
                ParentId = folder.ParentId,
                CategoryType = folder.CategoryType,
                FullPath = folder.FullPath,
                SubFolders = folder.SubFolders?.Select(f => f.ToViewModel()).ToList() ?? new List<FolderViewModel>()
            };
        }
        public static List<FolderViewModel> ToViewModel(this List<FolderObject> folders)
        {
            return folders.Select(
                folder => folder.ToViewModel())
                .ToList();
        }


        public static List<FolderViewModel> ToViewModel(this IEnumerable<DataExtensionFolder> folders)
        {
            return folders.Select(
                folder => folder.ToViewModel())
                .ToList();
        }

        public static FolderViewModel ToViewModel(this DataExtensionFolder folder)
        {
            return new FolderViewModel
            {
                Id = folder.Id,
                Description = folder.Description,
                EnterpriseId = folder.EnterpriseId,
                MemberId = folder.MemberId,
                Name = folder.Name,
                ParentId = folder.ParentId,
                //CategoryType = folder.ParentId,
                FullPath = folder.FullPath,
                SubFolders = folder.SubFolders?.Select(f => f.ToViewModel()).ToList() ?? new List<FolderViewModel>()
            };
        }

        public static List<DataExtensionViewModel> ToViewModel(this IEnumerable<DataExtensionPoco> dataextensions)
        {
            return dataextensions.Select(
                de => de.ToViewModel())
                .ToList();
        }
        public static DataExtensionViewModel ToViewModel(this DataExtensionPoco dataextension)
        {
            return new DataExtensionViewModel
            {
                CategoryId = dataextension.CategoryID,
                Name = dataextension.Name,
                CustomerKey = dataextension.CustomerKey,
                Description = dataextension.Description,
                IsSendable = dataextension.IsSendable,
                IsTestable = dataextension.IsTestable,
            };
        }
    }
}