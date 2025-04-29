using System.Collections.ObjectModel;

namespace SfmcApp;

public partial class MainPage : ContentPage
{
	public int count = 0;

	public ObservableCollection<TreeNode> Nodes { get; set; }

	public MainPage()
	{
		InitializeComponent();

		Nodes = new ObservableCollection<TreeNode>
        {
            new TreeNode
            {
                Name = "Parent A",
                Children = new List<TreeNode>
                {
                    new TreeNode { Name = "Child A1" },
                    new TreeNode { Name = "Child A2" }
                }
            },
            new TreeNode
            {
                Name = "Parent B",
                Children = new List<TreeNode>
                {
                    new TreeNode { Name = "Child B1" }
                }
            }
        };

        BindingContext = this;
	}

	private void OnCounterClicked(object sender, EventArgs e)
	{
		count++;

		if (count == 1)
			CounterBtn.Text = $"Clicked {count} time";
		else
			CounterBtn.Text = $"Clicked {count} times";

		SemanticScreenReader.Announce(CounterBtn.Text);
    }
}