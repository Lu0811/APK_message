using DebtMessageManager.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using System.Threading.Tasks;

namespace DebtMessageManager;

public partial class App : Application
{
    public static IServiceProvider Services { get; set; } = default!;

    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        _ = InitializeDatabaseAsync();
        return new Window(new AppShell());
    }

    private async Task InitializeDatabaseAsync()
    {
        try
        {
            var initializer = Services.GetService<DatabaseInitializer>();
            if (initializer is not null)
            {
                await initializer.InitializeAsync();
            }
        }
        catch (System.Exception)
        {
            // Manejo silencioso en el arranque inicial
        }
    }
}