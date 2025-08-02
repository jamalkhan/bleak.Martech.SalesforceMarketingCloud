//using CommunityToolkit.Mvvm.ComponentModel;

namespace SfmcApp.Models.ViewModels
{
    public interface IFolder
    {
        int Id { get; set; }
        string Name { get; set; }
    }

    public class FolderViewModel : IFolder
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
}