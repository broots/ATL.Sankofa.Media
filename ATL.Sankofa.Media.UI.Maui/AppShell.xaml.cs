using ATL.Sankofa.Media.UI.Maui.Views;

namespace ATL.Sankofa.Media.UI.Maui;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute("watch", typeof(WatchPage));
    }
}
