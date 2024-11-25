
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
        public List<FolderObject> SubFolders {get;set;}

        #region Helper Methods
         // Method to build the folder structure
        public static List<FolderObject> BuildFolderTree(List<SfmcFolder> sfmcFolders)
        {
            const int root_folder = 0;

            // Find root folders
            var sfmcRoots = sfmcFolders.Where(f => f.parentId == root_folder).ToList();
            var retval = new List<FolderObject>();
            foreach (var sfmcRoot in sfmcRoots)
            {
                var folder = sfmcRoot.ToFolderObject();
                folder.FullPath = "/";
                AddChildren(folder, sfmcFolders);
                retval.Add(folder);
            }
            

            return retval;
        }

        private static void AddChildren(FolderObject parentFolder, List<SfmcFolder> sfmcFolders)
        {
            var sfmcFolders_w_MatchingParentId = sfmcFolders.Where(x => x.parentId == parentFolder.Id).ToList();
            if (sfmcFolders_w_MatchingParentId.Any())
            {
                parentFolder.SubFolders = new List<FolderObject>();
                foreach (var sfmcFolder in sfmcFolders_w_MatchingParentId)
                {
                    var subfolder = sfmcFolder.ToFolderObject();
                    subfolder.FullPath = parentFolder.FullPath + "/" + subfolder.Name;
                    AddChildren(subfolder, sfmcFolders);
                    parentFolder.SubFolders.Add(subfolder);
                }
            }
        }

        // Helper method to print the structure for debugging (optional)
        public void PrintStructure(int indent = 0)
        {
            Console.WriteLine($"{FullPath}");
            /*for (int i = 0; i < indent; i++)
            {
                Console.Write(" ");
            }
            Console.Write($"{Name}{Environment.NewLine}");
            */
            if (SubFolders != null && SubFolders.Any())
            {
                foreach (var subFolder in SubFolders)
                {
                    subFolder.PrintStructure(indent + 1);
                }
            }
        }

        #endregion Helper Methods
    }
}