using DebtMessageManager.Data.Repositories;
using DebtMessageManager.Models;
using DebtMessageManager.Services.Messaging;
using DebtMessageManager.Services.Templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DebtMessageManager.Services.Automation;

public class AutomationService : IAutomationService
{
    private readonly ClienteRepository _clienteRepository;
    private readonly TelefonoRepository _telefonoRepository;
    private readonly ReglaRepository _reglaRepository;
    private readonly PlantillaRepository _plantillaRepository;
    private readonly ConfiguracionRepository _configuracionRepository;
    private readonly MensajeRepository _mensajeRepository;
    private readonly IMessageTemplateService _templateService;
    private readonly ISmsService _smsService;

    public AutomationService(
        ClienteRepository clienteRepository,
        TelefonoRepository telefonoRepository,
        ReglaRepository reglaRepository,
        PlantillaRepository plantillaRepository,
        ConfiguracionRepository configuracionRepository,
        MensajeRepository mensajeRepository,
        IMessageTemplateService templateService,
        ISmsService smsService)
    {
        _clienteRepository = clienteRepository;
        _telefonoRepository = telefonoRepository;
        _reglaRepository = reglaRepository;
        _plantillaRepository = plantillaRepository;
        _configuracionRepository = configuracionRepository;
        _mensajeRepository = mensajeRepository;
        _templateService = templateService;
        _smsService = smsService;
    }

    public bool IsWithinOperatingHours(TimeSpan currentTime, TimeSpan startTime, TimeSpan endTime)
    {
        if (startTime <= endTime)
        {
            return currentTime >= startTime && currentTime <= endTime;
        }
        else
        {
            // Cruza la medianoche
            return currentTime >= startTime || currentTime <= endTime;
        }
    }

    public async Task<AutomationPreviewResult> EvaluateCampaignAsync()
    {
        var preview = new AutomationPreviewResult();
        var hoy = DateTime.Today;
        var horaActual = DateTime.Now.TimeOfDay;

        var config = await _configuracionRepository.GetConfiguracionAsync();
        bool enHorario = IsWithinOperatingHours(horaActual, config.HoraInicio, config.HoraFin);
        preview.EstaFueraDeHorario = !enHorario;

        if (!config.AutomatizacionActiva)
        {
            preview.Advertencias.Add("La automatización general se encuentra DESACTIVADA en configuración.");
        }

        var clientes = await _clienteRepository.GetClientesAsync(includeTelefonos: true);
        var reglasActivas = await _reglaRepository.GetReglasActivasAsync();
        var plantillas = await _plantillaRepository.GetPlantillasAsync();
        var plantillasDict = plantillas.ToDictionary(p => p.Id);

        preview.TotalClientesEvaluados = clientes.Count;

        foreach (var cliente in clientes)
        {
            // Regla 1: Sin deuda
            if (cliente.MontoDeuda <= 0)
            {
                preview.ClientesSinDeuda++;
                continue;
            }

            var telefonosActivos = cliente.Telefonos.Where(t => t.Activo).ToList();
            if (telefonosActivos.Count == 0)
            {
                preview.ClientesNoCorresponde++;
                continue;
            }

            ReglaEnvio? reglaSeleccionada = null;

            // Regla 2: Deuda vigente (Antes del vencimiento)
            if (hoy <= cliente.FechaVencimiento.Date)
            {
                int diasFaltantes = (cliente.FechaVencimiento.Date - hoy).Days;

                var reglaAntes = reglasActivas
                    .Where(r => r.TipoEnum == TipoReglaEnvio.AntesVencimiento && r.Dias == diasFaltantes)
                    .FirstOrDefault();

                if (reglaAntes is not null)
                {
                    bool yaEnviado = await _mensajeRepository.ExisteMensajeEnviadoAsync(cliente.Id, reglaAntes.Id, hoy);
                    if (!yaEnviado)
                    {
                        reglaSeleccionada = reglaAntes;
                    }
                }
            }
            // Regla 3: Deuda vencida (Después del vencimiento)
            else
            {
                int diasRetraso = (hoy - cliente.FechaVencimiento.Date).Days;

                // Verificar período de gracia
                if (diasRetraso >= config.DiasGracia)
                {
                    // Buscar reglas que apliquen para los días de atraso actuales, ordenadas de mayor a menor
                    var reglasDespues = reglasActivas
                        .Where(r => r.TipoEnum == TipoReglaEnvio.DespuesVencimiento && diasRetraso >= r.Dias)
                        .OrderByDescending(r => r.Dias)
                        .ToList();

                    foreach (var r in reglasDespues)
                    {
                        bool yaEnviado = await _mensajeRepository.ExisteMensajeEnviadoAsync(cliente.Id, r.Id, hoy);
                        if (!yaEnviado)
                        {
                            reglaSeleccionada = r;
                            break; // Tomar la regla más relevante que no se haya enviado aún
                        }
                    }
                }
            }

            if (reglaSeleccionada is not null && plantillasDict.TryGetValue(reglaSeleccionada.PlantillaId, out var plantilla))
            {
                string mensajeTexto = _templateService.GenerateMessage(plantilla.Contenido, cliente);

                foreach (var tel in telefonosActivos)
                {
                    preview.Candidatos.Add(new AutomationCandidate
                    {
                        Cliente = cliente,
                        Telefono = tel,
                        Regla = reglaSeleccionada,
                        Plantilla = plantilla,
                        MensajeTexto = mensajeTexto,
                        EsFueraDeHorario = !enHorario
                    });
                }
            }
            else
            {
                preview.ClientesNoCorresponde++;
            }
        }

        return preview;
    }

    public async Task<AutomationExecutionResult> ExecuteCampaignAsync(AutomationPreviewResult preview)
    {
        var result = new AutomationExecutionResult();
        result.TotalProcesados = preview.Candidatos.Count;

        foreach (var candidato in preview.Candidatos)
        {
            var nuevoMensaje = new Mensaje
            {
                ClienteId = candidato.Cliente.Id,
                TelefonoId = candidato.Telefono.Id,
                PlantillaId = candidato.Plantilla.Id,
                ReglaId = candidato.Regla.Id,
                ClienteNombre = candidato.Cliente.Nombre,
                NumeroDestino = candidato.Telefono.Numero,
                Contenido = candidato.MensajeTexto,
                FechaProgramada = DateTime.Now
            };

            // Si está fuera de horario, se programa en vez de enviar de inmediato
            if (candidato.EsFueraDeHorario)
            {
                nuevoMensaje.EstadoEnum = EstadoMensaje.Programado;
                nuevoMensaje.Error = "Programado fuera del horario permitido";
                await _mensajeRepository.SaveMensajeAsync(nuevoMensaje);
                result.Programados++;
            }
            else
            {
                nuevoMensaje.EstadoEnum = EstadoMensaje.Enviando;
                var smsResult = await _smsService.SendSmsAsync(candidato.Telefono.Numero, candidato.MensajeTexto);

                if (smsResult.IsSuccess)
                {
                    nuevoMensaje.EstadoEnum = EstadoMensaje.Enviado;
                    nuevoMensaje.FechaEnvio = DateTime.Now;
                    nuevoMensaje.Error = string.Empty;
                    result.EnviadosExitosos++;
                }
                else
                {
                    nuevoMensaje.EstadoEnum = EstadoMensaje.Error;
                    nuevoMensaje.Error = smsResult.ErrorMessage;
                    result.Errores++;
                    result.MensajesError.Add($"Error enviando a {candidato.Cliente.Nombre} ({candidato.Telefono.Numero}): {smsResult.ErrorMessage}");
                }

                await _mensajeRepository.SaveMensajeAsync(nuevoMensaje);
            }
        }

        return result;
    }

    public async Task<AutomationExecutionResult> RetryFailedMessagesAsync()
    {
        var result = new AutomationExecutionResult();
        var mensajesError = await _mensajeRepository.GetMensajesByEstadoAsync(EstadoMensaje.Error);
        result.TotalProcesados = mensajesError.Count;

        foreach (var mensaje in mensajesError)
        {
            var smsResult = await _smsService.SendSmsAsync(mensaje.NumeroDestino, mensaje.Contenido);

            if (smsResult.IsSuccess)
            {
                mensaje.EstadoEnum = EstadoMensaje.Enviado;
                mensaje.FechaEnvio = DateTime.Now;
                mensaje.Error = string.Empty;
                result.EnviadosExitosos++;
            }
            else
            {
                mensaje.EstadoEnum = EstadoMensaje.Error;
                mensaje.Error = smsResult.ErrorMessage;
                result.Errores++;
                result.MensajesError.Add($"Reintento fallido para {mensaje.ClienteNombre} ({mensaje.NumeroDestino}): {smsResult.ErrorMessage}");
            }

            await _mensajeRepository.SaveMensajeAsync(mensaje);
        }

        return result;
    }
}

