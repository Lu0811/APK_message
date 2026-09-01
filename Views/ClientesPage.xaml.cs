using DebtMessageManager.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;

namespace DebtMessageManager.Views;

public partial class ClientesPage : ContentPage
{
    private readonly ClientesViewModel _viewModel;

    public ClientesPage()
    {
        InitializeComponent();
        _viewModel = App.Services.GetRequiredService<ClientesViewModel>();
        BindingContext = _viewModel;
    }

    public ClientesPage(ClientesViewModel viewModel)
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

