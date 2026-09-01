using SQLite;
using System;

namespace DebtMessageManager.Models;

public class ReglaEnvio
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Tipo { get; set; } = TipoReglaEnvio.DespuesVencimiento.ToString();

    public int Dias { get; set; }

    public int PlantillaId { get; set; }

    public bool Activa { get; set; } = true;

    [Ignore]
    public TipoReglaEnvio TipoEnum
    {
        get => Enum.TryParse<TipoReglaEnvio>(Tipo, out var val) ? val : TipoReglaEnvio.DespuesVencimiento;
        set => Tipo = value.ToString();
    }

    [Ignore]
    public string DescripcionRegla => TipoEnum == TipoReglaEnvio.AntesVencimiento
        ? $"{Dias} día(s) antes del vencimiento"
        : $"{Dias} día(s) después del vencimiento";
}

