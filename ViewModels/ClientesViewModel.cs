using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DebtMessageManager.Data.Repositories;
using DebtMessageManager.Models;
using DebtMessageManager.Views;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace DebtMessageManager.ViewModels;

public partial class ClientesViewModel : ObservableObject
{
    private readonly ClienteRepository _clienteRepository;
    private List<Cliente> _todosLosClientes = new();

    [ObservableProperty]
    public partial string TextoBusqueda { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string FiltroSeleccionado { get; set; } = "Todos";

    [ObservableProperty]
    public partial string ResumenConteo { get; set; } = "Cargando...";

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    public ObservableCollection<Cliente> ClientesFiltrados { get; } = new();

    public ClientesViewModel(ClienteRepository clienteRepository)
    {
        _clienteRepository = clienteRepository;
    }

    [RelayCommand]
    public async Task CargarAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            _todosLosClientes = await _clienteRepository.GetClientesAsync(includeTelefonos: true);
            AplicarFiltros();
        }
        catch (Exception ex)
        {
            ResumenConteo = $"Error al cargar: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnTextoBusquedaChanged(string value)
    {
        AplicarFiltros();
    }

    [RelayCommand]
    public void CambiarFiltro(string nuevoFiltro)
    {
        FiltroSeleccionado = nuevoFiltro;
        AplicarFiltros();
    }

    private void AplicarFiltros()
    {
        var query = _todosLosClientes.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(TextoBusqueda))
        {
            string b = TextoBusqueda.Trim();
            query = query.Where(c => 
                c.Nombre.Contains(b, StringComparison.OrdinalIgnoreCase) || 
                c.Telefonos.Any(t => t.Numero.Contains(b)));
        }

        query = FiltroSeleccionado switch
        {
            "Sin Deuda" => query.Where(c => c.EstadoCalculado == EstadoDeuda.SinDeuda),
            "Vigentes" => query.Where(c => c.EstadoCalculado == EstadoDeuda.Vigente),
            "Vencidos" => query.Where(c => c.EstadoCalculado == EstadoDeuda.Vencida),
            _ => query
        };

        var ordenados = query.OrderBy(c => c.Nombre).ToList();

        ClientesFiltrados.Clear();
        foreach (var item in ordenados)
        {
            ClientesFiltrados.Add(item);
        }

        ResumenConteo = $"Mostrando {ClientesFiltrados.Count} de {_todosLosClientes.Count} clientes";
    }

    [RelayCommand]
    public async Task AbrirClienteAsync(Cliente? cliente)
    {
        if (cliente is null || Shell.Current is null) return;
        await Shell.Current.GoToAsync($"{nameof(ClienteDetallePage)}?clienteId={cliente.Id}");
    }

    [RelayCommand]
    public async Task EliminarClienteAsync(Cliente? cliente)
    {
        if (cliente is null || Shell.Current is null) return;

        bool confirmar = await Shell.Current.DisplayAlertAsync(
            "Confirmar eliminación",
            $"¿Deseas eliminar a {cliente.Nombre} y sus teléfonos registrados?",
            "ELIMINAR",
            "CANCELAR");

        if (confirmar)
        {
            await _clienteRepository.DeleteClienteAsync(cliente);
            _todosLosClientes.Remove(cliente);
            AplicarFiltros();
        }
    }
}

