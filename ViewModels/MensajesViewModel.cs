using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DebtMessageManager.Data.Repositories;
using DebtMessageManager.Models;
using DebtMessageManager.Services.Automation;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace DebtMessageManager.ViewModels;

public partial class MensajesViewModel : ObservableObject
{
    private readonly MensajeRepository _mensajeRepository;
    private readonly IAutomationService _automationService;
    private List<Mensaje> _todosLosMensajes = new();

    [ObservableProperty]
    public partial string FiltroSeleccionado { get; set; } = "Todos";

    [ObservableProperty]
    public partial string ResumenConteo { get; set; } = "Cargando mensajes...";

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial bool TieneErrores { get; set; }

    public ObservableCollection<Mensaje> MensajesFiltrados { get; } = new();

    public MensajesViewModel(
        MensajeRepository mensajeRepository,
        IAutomationService automationService)
    {
        _mensajeRepository = mensajeRepository;
        _automationService = automationService;
    }

    [RelayCommand]
    public async Task CargarAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            _todosLosMensajes = await _mensajeRepository.GetMensajesAsync();
            TieneErrores = _todosLosMensajes.Any(m => m.EstadoEnum == EstadoMensaje.Error);
            AplicarFiltros();
        }
        catch (Exception ex)
        {
            ResumenConteo = $"Error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public void CambiarFiltro(string nuevoFiltro)
    {
        FiltroSeleccionado = nuevoFiltro;
        AplicarFiltros();
    }

    private void AplicarFiltros()
    {
        var query = _todosLosMensajes.AsEnumerable();

        query = FiltroSeleccionado switch
        {
            "Enviados" => query.Where(m => m.EstadoEnum == EstadoMensaje.Enviado),
            "Programados" => query.Where(m => m.EstadoEnum == EstadoMensaje.Programado),
            "Errores" => query.Where(m => m.EstadoEnum == EstadoMensaje.Error),
            _ => query
        };

        MensajesFiltrados.Clear();
        foreach (var item in query)
        {
            MensajesFiltrados.Add(item);
        }

        ResumenConteo = $"Mostrando {MensajesFiltrados.Count} de {_todosLosMensajes.Count} registros";
    }

    [RelayCommand]
    public async Task ReintentarFallidosAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var res = await _automationService.RetryFailedMessagesAsync();
            if (Shell.Current is not null)
            {
                await Shell.Current.DisplayAlertAsync(
                    "Reintento de Envíos",
                    $"Reintentados: {res.TotalProcesados}\n✓ Exitosos: {res.EnviadosExitosos}\n❌ Errores: {res.Errores}",
                    "OK");
            }
            await CargarAsync();
        }
        catch (Exception ex)
        {
            if (Shell.Current is not null)
                await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task LimpiarHistorialAsync()
    {
        if (Shell.Current is null) return;

        bool confirmar = await Shell.Current.DisplayAlertAsync(
            "Limpiar Historial",
            "¿Estás seguro de que deseas eliminar todo el registro de mensajes enviados?",
            "SÍ, LIMPIAR",
            "CANCELAR");

        if (confirmar)
        {
            await _mensajeRepository.ClearMensajesAsync();
            await CargarAsync();
        }
    }
}

