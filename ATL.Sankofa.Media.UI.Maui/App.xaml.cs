namespace ATL.Sankofa.Media.UI.Maui;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new MainPage()) { Title = "ATL.Sankofa.Media.UI.Maui" };
	}
}
