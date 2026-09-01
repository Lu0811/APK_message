using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DebtMessageManager.Data.Repositories;
using DebtMessageManager.Helpers;
using DebtMessageManager.Models;
using DebtMessageManager.Services.Automation;
using DebtMessageManager.Services.Export;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Controls;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DebtMessageManager.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly ClienteRepository _clienteRepository;
    private readonly MensajeRepository _mensajeRepository;
    private readonly IAutomationService _automationService;
    private readonly IExportService _exportService;

    [ObservableProperty]
    public partial int TotalClientes { get; set; }

    [ObservableProperty]
    public partial int SinDeudaCount { get; set; }

    [ObservableProperty]
    public partial int VigentesCount { get; set; }

    [ObservableProperty]
    public partial int VencidosCount { get; set; }

    [ObservableProperty]
    public partial int MensajesPendientesCount { get; set; }

    [ObservableProperty]
    public partial int MensajesEnviadosCount { get; set; }

    [ObservableProperty]
    public partial int MensajesErroresCount { get; set; }

    [ObservableProperty]
    public partial string MontoTotalDeuda { get; set; } = "S/ 0.00";

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = string.Empty;

    public DashboardViewModel(
        ClienteRepository clienteRepository,
        MensajeRepository mensajeRepository,
        IAutomationService automationService,
        IExportService exportService)
    {
        _clienteRepository = clienteRepository;
        _mensajeRepository = mensajeRepository;
        _automationService = automationService;
        _exportService = exportService;
    }

    [RelayCommand]
    public async Task CargarDashboardAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var clientes = await _clienteRepository.GetClientesAsync(includeTelefonos: false);
            TotalClientes = clientes.Count;

            SinDeudaCount = clientes.Count(c => c.EstadoCalculado == EstadoDeuda.SinDeuda);
            VigentesCount = clientes.Count(c => c.EstadoCalculado == EstadoDeuda.Vigente);
            VencidosCount = clientes.Count(c => c.EstadoCalculado == EstadoDeuda.Vencida);

            decimal sumaDeuda = clientes.Where(c => c.MontoDeuda > 0).Sum(c => c.MontoDeuda);
            MontoTotalDeuda = CurrencyHelper.FormatCurrency(sumaDeuda);

            var mensajes = await _mensajeRepository.GetMensajesAsync();
            MensajesPendientesCount = mensajes.Count(m => m.EstadoEnum == EstadoMensaje.Pendiente || m.EstadoEnum == EstadoMensaje.Programado);
            MensajesEnviadosCount = mensajes.Count(m => m.EstadoEnum == EstadoMensaje.Enviado);
            MensajesErroresCount = mensajes.Count(m => m.EstadoEnum == EstadoMensaje.Error);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error al cargar datos: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task EjecutarAutomatizacionAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        StatusMessage = "Evaluando reglas de cobranza...";

        try
        {
            var preview = await _automationService.EvaluateCampaignAsync();

            string mensajeResumen = $"Clientes evaluados: {preview.TotalClientesEvaluados}\n" +
                                   $"• Sin deuda: {preview.ClientesSinDeuda}\n" +
                                   $"• No corresponde envío: {preview.ClientesNoCorresponde}\n" +
                                   $"• Mensajes a procesar: {preview.MensajesAEnviar}\n" +
                                   $"• Números telefónicos: {preview.TelefonosDestino}\n\n";

            if (preview.EstaFueraDeHorario)
            {
                mensajeResumen += "⚠️ AVISO: Se encuentra FUERA del horario configurado. Los mensajes se guardarán con estado PROGRAMADO.\n\n";
            }

            if (preview.Advertencias.Count > 0)
            {
                mensajeResumen += string.Join("\n", preview.Advertencias) + "\n\n";
            }

            mensajeResumen += "¿Deseas continuar con el proceso?";

            if (Shell.Current is not null)
            {
                bool aceptar = await Shell.Current.DisplayAlertAsync(
                    "RESUMEN DE ENVÍO",
                    mensajeResumen,
                    "INICIAR PROCESAMIENTO",
                    "CANCELAR");

                if (aceptar)
                {
                    StatusMessage = "Procesando envíos...";
                    var result = await _automationService.ExecuteCampaignAsync(preview);

                    string resumenFinal = $"Proceso finalizado:\n" +
                                          $"✓ Enviados con éxito: {result.EnviadosExitosos}\n" +
                                          $"🕒 Programados: {result.Programados}\n" +
                                          $"❌ Errores: {result.Errores}";

                    await Shell.Current.DisplayAlertAsync("Resultado de Automatización", resumenFinal, "Aceptar");
                }
            }

            await CargarDashboardAsync();
        }
        catch (Exception ex)
        {
            if (Shell.Current is not null)
            {
                await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
            }
        }
        finally
        {
            IsBusy = false;
            StatusMessage = string.Empty;
        }
    }

    [RelayCommand]
    public async Task ExportarCsvAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            string path = await _exportService.ExportClientsToCsvAsync();
            if (Shell.Current is not null)
            {
                bool compartir = await Shell.Current.DisplayAlertAsync(
                    "Exportación Exitosa",
                    $"Archivo generado en:\n{path}\n\n¿Deseas compartir o abrir el archivo?",
                    "COMPARTIR",
                    "CERRAR");

                if (compartir)
                {
                    await Share.Default.RequestAsync(new ShareFileRequest
                    {
                        Title = "Exportación de Cobranzas",
                        File = new ShareFile(path)
                    });
                }
            }
        }
        catch (Exception ex)
        {
            if (Shell.Current is not null)
            {
                await Shell.Current.DisplayAlertAsync("Error al exportar", ex.Message, "OK");
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task IrAImportacionAsync()
    {
        if (Shell.Current is not null)
            await Shell.Current.GoToAsync("///ImportPage");
    }

    [RelayCommand]
    public async Task IrAClientesAsync()
    {
        if (Shell.Current is not null)
            await Shell.Current.GoToAsync("///ClientesPage");
    }

    [RelayCommand]
    public async Task IrAMensajesAsync()
    {
        if (Shell.Current is not null)
            await Shell.Current.GoToAsync("///MensajesPage");
    }

    [RelayCommand]
    public async Task IrAConfiguracionAsync()
    {
        if (Shell.Current is not null)
            await Shell.Current.GoToAsync("///ConfiguracionPage");
    }
}

