using ATL.Sankofa.Media.UI.Maui.ViewModels;

namespace ATL.Sankofa.Media.UI.Maui.Views;

public partial class HomePage : ContentPage
{
    private readonly HomeViewModel _viewModel;

    public HomePage(HomeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_viewModel.Videos.Count == 0)
            await _viewModel.LoadDataCommand.ExecuteAsync(null);
    }
}
