using DebtMessageManager.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;

namespace DebtMessageManager.Views;

public partial class ImportPage : ContentPage
{
    public ImportPage()
    {
        InitializeComponent();
        BindingContext = App.Services.GetRequiredService<ImportViewModel>();
    }

    public ImportPage(ImportViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}

