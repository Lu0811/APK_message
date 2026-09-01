using SQLite;
using System;
using System.Collections.Generic;

namespace DebtMessageManager.Models;

public class Cliente
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public string Nombre { get; set; } = string.Empty;

    public decimal MontoDeuda { get; set; }

    public DateTime FechaVencimiento { get; set; }

    public DateTime FechaImportacion { get; set; } = DateTime.Now;

    [Ignore]
    public EstadoDeuda EstadoCalculado
    {
        get
        {
            if (MontoDeuda <= 0)
                return EstadoDeuda.SinDeuda;

            return DateTime.Today <= FechaVencimiento.Date 
                ? EstadoDeuda.Vigente 
                : EstadoDeuda.Vencida;
        }
    }

    [Ignore]
    public int DiasRetraso
    {
        get
        {
            if (MontoDeuda <= 0 || DateTime.Today <= FechaVencimiento.Date)
                return 0;

            return (DateTime.Today - FechaVencimiento.Date).Days;
        }
    }

    [Ignore]
    public string EstadoTexto => EstadoCalculado switch
    {
        EstadoDeuda.SinDeuda => "Sin Deuda",
        EstadoDeuda.Vigente => "Vigente",
        EstadoDeuda.Vencida => "Vencida",
        _ => "Desconocido"
    };

    [Ignore]
    public string EstadoBadgeColor => EstadoCalculado switch
    {
        EstadoDeuda.SinDeuda => "#10B981", // Verde
        EstadoDeuda.Vigente => "#F59E0B",  // Amarillo / Ámbar
        EstadoDeuda.Vencida => "#EF4444",  // Rojo
        _ => "#6B7280"
    };

    [Ignore]
    public string MontoFormateado => $"S/ {MontoDeuda:N2}";

    [Ignore]
    public string FechaVencimientoFormateada => FechaVencimiento.ToString("dd/MM/yyyy");

    [Ignore]
    public List<Telefono> Telefonos { get; set; } = new();

    [Ignore]
    public string TelefonosResumen => Telefonos.Count > 0 
        ? string.Join(", ", Telefonos.Select(t => t.Numero)) 
        : "Sin teléfonos";
}

