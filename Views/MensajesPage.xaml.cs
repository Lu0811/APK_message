using DebtMessageManager.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;

namespace DebtMessageManager.Views;

public partial class MensajesPage : ContentPage
{
    private readonly MensajesViewModel _viewModel;

    public MensajesPage()
    {
        InitializeComponent();
        _viewModel = App.Services.GetRequiredService<MensajesViewModel>();
        BindingContext = _viewModel;
    }

    public MensajesPage(MensajesViewModel viewModel)
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

