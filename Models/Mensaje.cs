using SQLite;
using System;

namespace DebtMessageManager.Models;

public class Mensaje
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int ClienteId { get; set; }

    public int TelefonoId { get; set; }

    public int PlantillaId { get; set; }

    [Indexed]
    public int ReglaId { get; set; }

    public string ClienteNombre { get; set; } = string.Empty;

    public string NumeroDestino { get; set; } = string.Empty;

    public string Contenido { get; set; } = string.Empty;

    public DateTime FechaProgramada { get; set; } = DateTime.Now;

    public DateTime? FechaEnvio { get; set; }

    [Indexed]
    public string Estado { get; set; } = EstadoMensaje.Pendiente.ToString();

    public string Error { get; set; } = string.Empty;

    [Ignore]
    public EstadoMensaje EstadoEnum
    {
        get => Enum.TryParse<EstadoMensaje>(Estado, out var val) ? val : EstadoMensaje.Pendiente;
        set => Estado = value.ToString();
    }

    [Ignore]
    public string EstadoBadgeColor => EstadoEnum switch
    {
        EstadoMensaje.Enviado => "#10B981",    // Verde
        EstadoMensaje.Programado => "#3B82F6", // Azul
        EstadoMensaje.Pendiente => "#F59E0B",  // Ámbar
        EstadoMensaje.Enviando => "#8B5CF6",   // Violeta
        EstadoMensaje.Error => "#EF4444",      // Rojo
        _ => "#6B7280"
    };

    [Ignore]
    public string FechaMostrar => FechaEnvio.HasValue 
        ? FechaEnvio.Value.ToString("dd/MM/yyyy HH:mm") 
        : FechaProgramada.ToString("dd/MM/yyyy HH:mm");
}

