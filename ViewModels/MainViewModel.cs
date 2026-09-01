using CommunityToolkit.Mvvm.ComponentModel;

namespace DebtMessageManager.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string Titulo { get; set; } = "Debt Message Manager";

    [ObservableProperty]
    public partial string Subtitulo { get; set; } = "Gestión local de cobranzas con CSV y SQLite";
}

