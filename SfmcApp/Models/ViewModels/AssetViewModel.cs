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
        public static List<AssetViewModel> ToViewModelList(this List<AssetPoco> assets)
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

    }

    /// <summary>
    /// ViewModel for AssetPoco, extending the AssetPoco class.
    /// </summary>
    public class AssetViewModel : AssetPoco
    {
        /// <summary>
        /// Initializes a new instance of the AssetViewModel class.
        /// </summary>
        public AssetViewModel()
        {
            // Initialize any additional properties or collections if needed.
        }
        public string Icon
        {
            get
            {
                return AssetType?.Name switch
                {
                    // html assets
                    "webpage" => "webpage.png",
                    "htmlemail" => "htmlemail.png",
                    "htmlblock" => "htmlblock.png",
                    "templatebasedemail" => "templatebasedemail.png",
                    "template" => "template.png",
                    // code blocks
                    "codesnippetblock" => "codesnippetblock.png",
                    "defaulttemplate" => "defaulttemplate.png",
                    "freeformblock" => "freeform.png",
                    // images
                    "jpg" => "jpg.png",
                    "png" => "png.png",
                    "gif" => "gif.png",
                    _ => "question.png"
                };
            }
        }
    }
}