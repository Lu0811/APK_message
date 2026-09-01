using DebtMessageManager.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;

namespace DebtMessageManager.Views;

[QueryProperty(nameof(ClienteId), "clienteId")]
public partial class ClienteDetallePage : ContentPage
{
    private readonly ClienteDetalleViewModel _viewModel;
    private int _clienteId;

    public ClienteDetallePage()
    {
        InitializeComponent();
        _viewModel = App.Services.GetRequiredService<ClienteDetalleViewModel>();
        BindingContext = _viewModel;
    }

    public ClienteDetallePage(ClienteDetalleViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    public int ClienteId
    {
        get => _clienteId;
        set
        {
            _clienteId = value;
            _ = _viewModel.CargarAsync(value);
        }
    }
}

