using SfmcApp.Models.ViewModels;
using bleak.Martech.SalesforceMarketingCloud.Models;

namespace SfmcApp.Models.ViewModels.Converters
{
    public static class ObjectToViewModel
    {
        public static IEnumerable<FolderViewModel> ToViewModel(this IEnumerable<FolderObject> folders)
        {
            return folders.Select(folder => folder.ToViewModel());
        }
        
        public static FolderViewModel ToViewModel(this FolderObject folder)
        {
            var retval = new FolderViewModel
            {
                Id = folder.Id,
                Description = folder.Description ?? string.Empty,
                EnterpriseId = folder.EnterpriseId,
                MemberId = folder.MemberId,
                Name = folder.Name ?? string.Empty,
                ParentId = folder.ParentId,
                CategoryType = folder.CategoryType ?? string.Empty,
                FullPath = folder.FullPath ?? string.Empty,
                SubFolders = new List<FolderViewModel>()
            };
            foreach (var subFolder in folder.SubFolders)
            {
                retval.SubFolders.Add(subFolder.ToViewModel());
            }
            return retval;
        }
    }
}