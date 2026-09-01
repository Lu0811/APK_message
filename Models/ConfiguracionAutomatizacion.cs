using SQLite;
using System;

namespace DebtMessageManager.Models;

public class ConfiguracionAutomatizacion
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public TimeSpan HoraInicio { get; set; } = new TimeSpan(8, 0, 0);

    public TimeSpan HoraFin { get; set; } = new TimeSpan(18, 0, 0);

    public int DiasGracia { get; set; } = 3;

    public bool AutomatizacionActiva { get; set; } = true;
}

