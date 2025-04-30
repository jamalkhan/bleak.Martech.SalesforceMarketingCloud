using System.Collections.Generic;
using System.Linq;

namespace bleak.Martech.SalesforceMarketingCloud.Models
{
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
        public List<FolderObject> SubFolders { get; set; } = [];
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