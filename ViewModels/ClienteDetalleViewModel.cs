using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DebtMessageManager.Data.Repositories;
using DebtMessageManager.Helpers;
using DebtMessageManager.Models;
using DebtMessageManager.Services.Messaging;
using DebtMessageManager.Services.Templates;
using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace DebtMessageManager.ViewModels;

public partial class ClienteDetalleViewModel : ObservableObject
{
    private readonly ClienteRepository _clienteRepository;
    private readonly TelefonoRepository _telefonoRepository;
    private readonly MensajeRepository _mensajeRepository;
    private readonly PlantillaRepository _plantillaRepository;
    private readonly IMessageTemplateService _templateService;
    private readonly ISmsService _smsService;

    [ObservableProperty]
    public partial int ClienteId { get; set; }

    [ObservableProperty]
    public partial string Nombre { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string MontoDeuda { get; set; } = "S/ 0.00";

    [ObservableProperty]
    public partial string FechaVencimiento { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string EstadoTexto { get; set; } = "Desconocido";

    [ObservableProperty]
    public partial string EstadoBadgeColor { get; set; } = "#6B7280";

    [ObservableProperty]
    public partial string DiasRetrasoTexto { get; set; } = "0 días";

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial string NuevoNumero { get; set; } = string.Empty;

    private Cliente? _clienteActual;

    public ObservableCollection<Telefono> Telefonos { get; } = new();
    public ObservableCollection<Mensaje> Mensajes { get; } = new();

    public ClienteDetalleViewModel(
        ClienteRepository clienteRepository,
        TelefonoRepository telefonoRepository,
        MensajeRepository mensajeRepository,
        PlantillaRepository plantillaRepository,
        IMessageTemplateService templateService,
        ISmsService smsService)
    {
        _clienteRepository = clienteRepository;
        _telefonoRepository = telefonoRepository;
        _mensajeRepository = mensajeRepository;
        _plantillaRepository = plantillaRepository;
        _templateService = templateService;
        _smsService = smsService;
    }

    [RelayCommand]
    public async Task CargarAsync(int id)
    {
        if (id <= 0) return;
        ClienteId = id;
        IsBusy = true;

        try
        {
            _clienteActual = await _clienteRepository.GetClienteAsync(id, includeTelefonos: true);

            if (_clienteActual is null)
            {
                Nombre = "Cliente no encontrado";
                return;
            }

            Nombre = _clienteActual.Nombre;
            MontoDeuda = _clienteActual.MontoFormateado;
            FechaVencimiento = _clienteActual.FechaVencimientoFormateada;
            EstadoTexto = _clienteActual.EstadoTexto;
            EstadoBadgeColor = _clienteActual.EstadoBadgeColor;
            DiasRetrasoTexto = $"{_clienteActual.DiasRetraso} día(s)";

            Telefonos.Clear();
            foreach (var t in _clienteActual.Telefonos)
            {
                Telefonos.Add(t);
            }

            var historial = await _mensajeRepository.GetMensajesByClienteIdAsync(id);
            Mensajes.Clear();
            foreach (var m in historial)
            {
                Mensajes.Add(m);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task AlternarTelefonoActivoAsync(Telefono? tel)
    {
        if (tel is null) return;
        tel.Activo = !tel.Activo;
        await _telefonoRepository.SaveTelefonoAsync(tel);
        await CargarAsync(ClienteId);
    }

    [RelayCommand]
    public async Task AgregarTelefonoAsync()
    {
        if (string.IsNullOrWhiteSpace(NuevoNumero) || _clienteActual is null) return;

        string normalizado = PhoneNumberHelper.NormalizePhoneNumber(NuevoNumero);
        if (!PhoneNumberHelper.IsValidPhoneNumber(normalizado))
        {
            if (Shell.Current is not null)
                await Shell.Current.DisplayAlertAsync("Teléfono Inválido", "Ingrese un número válido.", "OK");
            return;
        }

        var nuevo = new Telefono
        {
            ClienteId = _clienteActual.Id,
            Numero = normalizado,
            Activo = true
        };

        await _telefonoRepository.SaveTelefonoAsync(nuevo);
        NuevoNumero = string.Empty;
        await CargarAsync(ClienteId);
    }

    [RelayCommand]
    public async Task EliminarTelefonoAsync(Telefono? tel)
    {
        if (tel is null) return;
        await _telefonoRepository.DeleteTelefonoAsync(tel);
        await CargarAsync(ClienteId);
    }

    [RelayCommand]
    public async Task EnviarSmsManualAsync(Telefono? tel)
    {
        if (_clienteActual is null || Shell.Current is null) return;

        var plantillas = await _plantillaRepository.GetPlantillasActivasAsync();
        if (plantillas.Count == 0)
        {
            await Shell.Current.DisplayAlertAsync("Sin Plantillas", "No hay plantillas de mensaje activas disponibles.", "OK");
            return;
        }

        string[] opciones = plantillas.Select(p => p.Nombre).ToArray();
        string seleccion = await Shell.Current.DisplayActionSheetAsync("Seleccionar Plantilla de Envío", "Cancelar", null, opciones);

        if (string.IsNullOrWhiteSpace(seleccion) || seleccion == "Cancelar") return;

        var plantillaElegida = plantillas.FirstOrDefault(p => p.Nombre == seleccion);
        if (plantillaElegida is null) return;

        string mensajeTexto = _templateService.GenerateMessage(plantillaElegida.Contenido, _clienteActual);

        string destino = tel?.Numero ?? Telefonos.FirstOrDefault()?.Numero ?? string.Empty;
        if (string.IsNullOrWhiteSpace(destino))
        {
            await Shell.Current.DisplayAlertAsync("Sin Teléfono", "El cliente no tiene teléfonos registrados.", "OK");
            return;
        }

        bool confirmar = await Shell.Current.DisplayAlertAsync(
            "Confirmar Envío SMS",
            $"Destino: {destino}\n\nMensaje:\n\"{mensajeTexto}\"",
            "ENVIAR AHORA",
            "CANCELAR");

        if (confirmar)
        {
            var res = await _smsService.SendSmsAsync(destino, mensajeTexto);

            var registro = new Mensaje
            {
                ClienteId = _clienteActual.Id,
                TelefonoId = tel?.Id ?? 0,
                PlantillaId = plantillaElegida.Id,
                ClienteNombre = _clienteActual.Nombre,
                NumeroDestino = destino,
                Contenido = mensajeTexto,
                FechaProgramada = DateTime.Now,
                FechaEnvio = res.IsSuccess ? DateTime.Now : null,
                EstadoEnum = res.IsSuccess ? EstadoMensaje.Enviado : EstadoMensaje.Error,
                Error = res.ErrorMessage
            };

            await _mensajeRepository.SaveMensajeAsync(registro);

            if (res.IsSuccess)
            {
                await Shell.Current.DisplayAlertAsync("SMS Enviado", "El mensaje fue enviado correctamente.", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlertAsync("Error en SMS", res.ErrorMessage, "OK");
            }

            await CargarAsync(ClienteId);
        }
    }
}

