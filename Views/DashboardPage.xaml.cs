using DebtMessageManager.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;

namespace DebtMessageManager.Views;

public partial class DashboardPage : ContentPage
{
    private readonly DashboardViewModel _viewModel;

    public DashboardPage()
    {
        InitializeComponent();
        _viewModel = App.Services.GetRequiredService<DashboardViewModel>();
        BindingContext = _viewModel;
    }

    public DashboardPage(DashboardViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.CargarDashboardAsync();
    }
}

