namespace SfmcApp;

public partial class SfmcConnectionEditPage : ContentPage
{
	public SfmcConnectionEditPage()
	{
		InitializeComponent();

        // Load existing values if any
        SubdomainEntry.Text = Preferences.Get("Subdomain", "");
        ClientIdEntry.Text = Preferences.Get("ClientId", "");
        ClientSecretEntry.Text = Preferences.Get("ClientSecret", "");
        MemberIdEntry.Text = Preferences.Get("MemberId", "");
    }

    private void OnSaveClicked(object sender, EventArgs e)
    {
        Preferences.Set("Subdomain", SubdomainEntry.Text);
        Preferences.Set("ClientId", ClientIdEntry.Text);
        Preferences.Set("ClientSecret", ClientSecretEntry.Text);
        Preferences.Set("MemberId", MemberIdEntry.Text);

        DisplayAlert("Saved", "SFMC credentials saved.", "OK");
    }
}