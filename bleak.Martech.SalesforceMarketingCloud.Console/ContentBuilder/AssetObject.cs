
using System.Collections.Generic;
using System.Linq;

namespace bleak.Martech.SalesforceMarketingCloud.ContentBuilder
{





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