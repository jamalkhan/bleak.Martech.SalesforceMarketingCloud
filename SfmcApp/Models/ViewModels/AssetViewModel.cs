using bleak.Martech.SalesforceMarketingCloud.Models.Pocos;
//using CommunityToolkit.Mvvm.ComponentModel;

namespace SfmcApp.Models.ViewModels
{
    public class FolderViewModel
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public int EnterpriseId { get; set; }
        public int MemberId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int ParentId { get; set; }
        public string CategoryType { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;
        public List<FolderViewModel> SubFolders { get; set; } = [];

    }

        /// <summary>
        /// ViewModel for AssetPoco, extending the AssetPoco class.
        /// </summary>
        public class AssetViewModel : AssetPoco
        {
            public bool HasContentType
            {
                get
                {
                    return !string.IsNullOrEmpty(this.ContentType);
                }
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
            public bool IsDownloading
            { get; set;
            } = false;

            public bool IsBinaryDownloadable
            {
                get
                {
                    return AssetType?.Name switch
                    {
                        "jpg" => true,
                        "png" => true,
                        "gif" => true,
                        _ => false
                    };
                }
            }

            public bool IsDownloadbale
            {
                get
                {
                    return AssetType?.Name switch
                    {
                        // html assets
                        "webpage" => true,
                        "htmlemail" => true,
                        "htmlblock" => true,
                        "templatebasedemail" => true,
                        "template" => true,
                        // code blocks
                        "codesnippetblock" => true,
                        "defaulttemplate" => true,
                        "freeformblock" => true,
                        // images
                        "jpg" => true,
                        "png" => true,
                        "gif" => true,
                        _ => false
                    };
                }
            }
        }
}