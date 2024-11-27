
using System.Collections.Generic;
using System.Linq;

namespace bleak.Martech.SalesforceMarketingCloud.ContentBuilder
{
    public class SfmcAssetRestWrapper
    {
        public int count { get; set; }
        public int page { get; set; }
        public int pageSize { get; set; }
        public Dictionary<string, object> links { get; set; } = new();
        public List<SfmcAsset> items { get; set; } = new();
    }

    public class SfmcAsset
    {
        public int id { get; set; }
        public string customerKey { get; set; } = string.Empty;
        public string objectID { get; set; } = string.Empty;
        public AssetType assetType { get; set; } = new();
        public string name { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public DateTime createdDate { get; set; }
        public User createdBy { get; set; } = new();
        public DateTime modifiedDate { get; set; }
        public User modifiedBy { get; set; } = new();
        public int enterpriseId { get; set; }
        public int memberId { get; set; }
        public Status status { get; set; } = new();
        public Thumbnail thumbnail { get; set; } = new();
        public Category category { get; set; } = new();
        public string content { get; set; } = string.Empty;
        public Data data { get; set; } = new();
        public Views views { get; set; } = new();
        public FileProperties fileProperties { get; set; } = new();

        public class FileProperties
        {
            public string fileName {get;set;} = string.Empty;
            public string extension {get;set;} = string.Empty;
            public int fileSize {get;set;}
            public DateTime fileCreatedDate {get;set;}
            public int width {get;set;}
            public int height {get;set;}
            public string publishedURL = string.Empty;
        }

        public class Views
        {
            public Html html {get;set; } = new();
        }
        public class Html
        {
            public string content {get;set;} = string.Empty;
        }


        public class AssetType
        {
            public int id { get; set; }
            public string name { get; set; } = string.Empty;
            public string displayName { get; set; } = string.Empty;
        }

        public class User
        {
            public int id { get; set; }
            public string email { get; set; } = string.Empty;
            public string name { get; set; } = string.Empty;
            public string userId { get; set; } = string.Empty;
        }

        public class Status
        {
            public int id { get; set; }
            public string name { get; set; } = string.Empty;
        }

        public class Thumbnail
        {
            public string thumbnailUrl { get; set; } = string.Empty;
        }

        public class Category
        {
            public int id { get; set; }
            public int parentId { get; set; }
            public string name { get; set; } = string.Empty;
        }

        public class Data
        {
            public Email email { get; set; } = new();
        }

        public class Email
        {
            public Options options { get; set; } = new();
        }

        public class Options
        {
            public object generateFrom { get; set; } = new();
        }


        public AssetObject ToAssetObject()
        {
            var retval = new AssetObject()
            {
                Id = id,
                CustomerKey = customerKey,
                ObjectID = objectID,
                AssetType = new AssetObject.AssetTypeObject()
                {
                    Id = assetType.id,
                    Name = assetType.name,
                    DisplayName = assetType.displayName 
                },
                Name = name,
                Description = description,
                CreatedDate = createdDate,
                // TODO
                // UserObject = null
                ModifiedDate = modifiedDate,
                // TODO:
                //ModifiedBy = modifiedBy,
                EnterpriseId= enterpriseId,
                MemberId = memberId,
                // TODO:
                //Status { get; set; } = new();
                // TODO:
                //Thumbnail { get; set; } = new();
                // TODO:
                //Category = new CategoryObject() { // TODO: }
                Content = content,
                //Data = new DataObject

            };
            if (views != null)
            {
                retval.Views = new AssetObject.ViewsObject();
                if (views.html != null)
                {
                    retval.Views.Html = new AssetObject.HtmlObject();
                    retval.Views.Html.Content = views.html.content;
                }
            }
            if (fileProperties != null)
            {
                retval.FileProperties = new AssetObject.FilePropertiesObject();
                retval.FileProperties.FileName = fileProperties.fileName;
                retval.FileProperties.Extension = fileProperties.extension;
                retval.FileProperties.FileSize = fileProperties.fileSize;
                retval.FileProperties.FileCreatedDate = fileProperties.fileCreatedDate;
                retval.FileProperties.Width = fileProperties.width;
                retval.FileProperties.Height = fileProperties.height;
                retval.FileProperties.PublishedURL = fileProperties.publishedURL;
            }

            return retval;
        }
    }





    public class AssetObject
    {
        public int Id { get; set; }
        public string CustomerKey { get; set; } = string.Empty;
        public string ObjectID { get; set; } = string.Empty;
        public AssetTypeObject AssetType { get; set; } = new();
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public UserObject CreatedBy { get; set; } = new();
        public DateTime ModifiedDate { get; set; }
        public UserObject ModifiedBy { get; set; } = new();
        public int EnterpriseId { get; set; }
        public int MemberId { get; set; }
        public StatusObject Status { get; set; } = new();
        public ThumbnailObject Thumbnail { get; set; } = new();
        public CategoryObject Category { get; set; } = new();
        public string Content { get; set; } = string.Empty;
        public DataObject Data { get; set; } = new();
        public FilePropertiesObject FileProperties { get;set; } = new();
        public ViewsObject Views { get;set; } = new();

        /// <summary>
        /// Full File Path as in SFMC
        /// </summary>
        public string FullPath { get; set; } = string.Empty;

        public class ViewsObject
        {
            public HtmlObject Html {get;set; } = new();
        }
        public class HtmlObject
        {
            public string Content {get;set;} = string.Empty;
        }

        public class FilePropertiesObject
        {
            public string FileName {get;set;} = string.Empty;
            public string Extension {get;set;} = string.Empty;
            public int FileSize {get;set;}
            public DateTime FileCreatedDate {get;set;}
            public int Width {get;set;}
            public int Height {get;set;}
            public string PublishedURL = string.Empty;
        }
        public class AssetTypeObject
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string DisplayName { get; set; } = string.Empty;
        }

        public class UserObject
        {
            public int Id { get; set; }
            public string Email { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string UserId { get; set; } = string.Empty;
        }

        public class StatusObject
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
        }

        public class ThumbnailObject
        {
            public string ThumbnailUrl { get; set; } = string.Empty;
        }

        public class CategoryObject
        {
            public int Id { get; set; }
            public int ParentId { get; set; }
            public string Name { get; set; } = string.Empty;
        }

        public class DataObject
        {
            public EmailObject Email { get; set; } = new();
        }

        public class EmailObject
        {
            public OptionsObject Options { get; set; } = new();
        }

        public class OptionsObject
        {
            public object GenerateFrom { get; set; } = new();
        }
    }


}