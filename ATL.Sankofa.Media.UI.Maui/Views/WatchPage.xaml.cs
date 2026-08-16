using ATL.Sankofa.Media.UI.Maui.ViewModels;

namespace ATL.Sankofa.Media.UI.Maui.Views;

public partial class WatchPage : ContentPage
{
	public WatchPage(WatchViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();
		Player.Stop();
		Player.Handler?.DisconnectHandler();
	}
}