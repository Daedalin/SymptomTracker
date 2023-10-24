namespace SymptomTracker;

public partial class LoginPage : ContentPage
{
	public LoginPage()
	{
		InitializeComponent();
	}
	
	protected override bool OnBackButtonPressed()
	{
		return false;
	}

}