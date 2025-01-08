using bleak.Api.Rest;
using bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Configuration;
using bleak.Martech.SalesforceMarketingCloud.ContentBuilder;
using bleak.Martech.SalesforceMarketingCloud.ContentBuilder.SfmcPocos;
using bleak.Martech.SalesforceMarketingCloud.Wsdl;
using System.Text;
using System.Security.Cryptography.Pkcs;

namespace bleak.Martech.SalesforceMarketingCloud.ConsoleApp.Sfmc.Soap.DataExtensions
{

    public class DataExtensionFolder : BasePoco
    {
        public int Id { get;set;}
        public int EnterpriseId { get; set; }
        public int MemberId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int ParentId { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsEditable { get; set; }
        public List<DataExtensionFolder> SubFolders { get; set; } = new List<DataExtensionFolder>();
        public List<DataExtensionPoco> DataExtensions { get; set; } = new List<DataExtensionPoco>();
    }
    
    public static class DataExtensionFolderExtensions
    {
        public static DataExtensionFolder ToDataExtensionFolder(this Wsdl.DataFolder folder)
        {
            return new DataExtensionFolder()
                    {
                        Id = folder.ID,
                        ParentId = folder.ParentFolder.ID,
                        Name = folder.Name,
                        Description = folder.Description,
                        ContentType = folder.ContentType,
                        IsActive = folder.IsActive,
                        IsEditable = folder.IsEditable
                    };
        }

        public static DataExtensionPoco ToDataExtensionPoco(this Wsdl.DataExtension dataExtension)
        {
            return new DataExtensionPoco()
                    {
                        ObjectID = dataExtension.ObjectID,
                        Name = dataExtension.Name,
                        Description = dataExtension.Description,
                        CustomerKey = dataExtension.CustomerKey,
                        CategoryID = dataExtension.CategoryID,
                        IsSendable = dataExtension.IsSendable,
                        IsTestable = dataExtension.IsTestable
                    };
        }

        public static QueryDefinitionPoco ToPoco(this Wsdl.QueryDefinition obj)
        {
            return new QueryDefinitionPoco()
                    {
                        Name = obj.Name,
                        Description = obj.Description,
                        CustomerKey = obj.CustomerKey,
                        DataExtensionTargetName = obj.DataExtensionTarget.Name,
                        QueryText = obj.QueryText,
                    };
        }
        
    }
}