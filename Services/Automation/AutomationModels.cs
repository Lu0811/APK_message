using DebtMessageManager.Models;
using System.Collections.Generic;

namespace DebtMessageManager.Services.Automation;

public class AutomationCandidate
{
    public Cliente Cliente { get; set; } = new();
    public Telefono Telefono { get; set; } = new();
    public ReglaEnvio Regla { get; set; } = new();
    public PlantillaMensaje Plantilla { get; set; } = new();
    public string MensajeTexto { get; set; } = string.Empty;
    public bool EsFueraDeHorario { get; set; }
}

public class AutomationPreviewResult
{
    public int TotalClientesEvaluados { get; set; }
    public int ClientesSinDeuda { get; set; }
    public int ClientesNoCorresponde { get; set; }
    public int MensajesAEnviar => Candidatos.Count;
    public int TelefonosDestino => Candidatos.Count;
    public bool EstaFueraDeHorario { get; set; }
    public List<AutomationCandidate> Candidatos { get; set; } = new();
    public List<string> Advertencias { get; set; } = new();
}

public class AutomationExecutionResult
{
    public int TotalProcesados { get; set; }
    public int EnviadosExitosos { get; set; }
    public int Programados { get; set; }
    public int Errores { get; set; }
    public List<string> MensajesError { get; set; } = new();
}

