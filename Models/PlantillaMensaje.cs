using SQLite;

namespace DebtMessageManager.Models;

public class PlantillaMensaje
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public string Nombre { get; set; } = string.Empty;

    public string Contenido { get; set; } = string.Empty;

    public bool Activa { get; set; } = true;
}

