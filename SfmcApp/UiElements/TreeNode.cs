namespace SfmcApp;

public class TreeNode
{
    public required string Name { get; set; }
    public List<TreeNode> Children { get; set; } = new List<TreeNode>();
    public bool HasChildren => Children != null && Children.Count > 0;
}