using DebtMessageManager.Views;

namespace DebtMessageManager;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(ClienteDetallePage), typeof(ClienteDetallePage));
    }
}

