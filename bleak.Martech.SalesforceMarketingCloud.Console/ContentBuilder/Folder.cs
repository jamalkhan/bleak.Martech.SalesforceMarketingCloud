
using System.Collections.Generic;
using System.Linq;

namespace bleak.Martech.SalesforceMarketingCloud.ContentBuilder
{
    public class SfmcFolderRestWrapper
    {
        public int count { get; set; }

        public int page { get; set; }

        public int pageSize { get; set; }

        public Dictionary<string, object> links { get; set; } = new();

        public List<SfmcFolder> items { get; set; } = new();
    }

    public class SfmcFolder
    {
        public int id { get; set; }

        public string description { get; set; } = string.Empty;

        public int enterpriseId { get; set; }

        public int memberId { get; set; }

        public string name { get; set; } = string.Empty;

        public int parentId { get; set; }

        public string categoryType { get; set; } = string.Empty;

        public FolderObject ToFolderObject()
        {
            return new FolderObject()
            {
                ParentId = parentId,
                Id = id,           
                Description = description,
                EnterpriseId = enterpriseId,
                MemberId = memberId,
                Name = name,
                CategoryType = categoryType
            };
        }
    }

    public class FolderObject
    {
        public int Id { get;set;}
        public string Description { get; set; } = string.Empty;
        public int EnterpriseId { get; set; }
        public int MemberId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int ParentId { get; set; }
        public string CategoryType { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;
        public List<FolderObject> SubFolders { get; set; } = new();
        public List<AssetObject> Assets { get; set; } = new();

        #region Helper Methods
         // Method to build the folder structure
        

        // Helper method to print the structure for debugging (optional)
        public void PrintStructure(int indent = 0)
        {
            Console.WriteLine($"{FullPath}");
            if (SubFolders != null && SubFolders.Any())
            {
                foreach (var subFolder in SubFolders)
                {
                    subFolder.PrintStructure(indent + 1);
                }
            }
        }

        public void PrintChildren()
        {
            Console.WriteLine($"{FullPath}");
            Console.WriteLine("-------- SubFolders ---------");
            foreach (var subfolder in SubFolders)
            {
                Console.WriteLine(subfolder.FullPath);
            }

            Console.WriteLine("-------- Assets ---------");
            foreach (var asset in Assets)
            {
                Console.WriteLine(asset.FullPath);
            }
        }

        #endregion Helper Methods
    }
}