using SQLite;

namespace DebtMessageManager.Models;

public class Telefono
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int ClienteId { get; set; }

    [Indexed]
    public string Numero { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;
}

