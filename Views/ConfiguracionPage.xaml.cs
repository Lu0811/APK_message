using DebtMessageManager.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;

namespace DebtMessageManager.Views;

public partial class ConfiguracionPage : ContentPage
{
    private readonly ConfiguracionViewModel _viewModel;

    public ConfiguracionPage()
    {
        InitializeComponent();
        _viewModel = App.Services.GetRequiredService<ConfiguracionViewModel>();
        BindingContext = _viewModel;
    }

    public ConfiguracionPage(ConfiguracionViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.CargarAsync();
    }
}

