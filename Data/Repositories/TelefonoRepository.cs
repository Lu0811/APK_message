using DebtMessageManager.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DebtMessageManager.Data.Repositories;

public class TelefonoRepository
{
    private readonly AppDatabase _database;

    public TelefonoRepository(AppDatabase database)
    {
        _database = database;
    }

    public async Task<List<Telefono>> GetAllTelefonosAsync()
    {
        var db = await _database.GetConnectionAsync();
        return await db.Table<Telefono>().ToListAsync();
    }

    public async Task<List<Telefono>> GetTelefonosByClienteIdAsync(int clienteId)
    {
        var db = await _database.GetConnectionAsync();
        return await db.Table<Telefono>().Where(t => t.ClienteId == clienteId).ToListAsync();
    }

    public async Task<List<Telefono>> GetTelefonosActivosByClienteIdAsync(int clienteId)
    {
        var db = await _database.GetConnectionAsync();
        return await db.Table<Telefono>().Where(t => t.ClienteId == clienteId && t.Activo).ToListAsync();
    }

    public async Task<int> SaveTelefonoAsync(Telefono item)
    {
        var db = await _database.GetConnectionAsync();
        if (item.Id != 0)
        {
            await db.UpdateAsync(item);
            return item.Id;
        }

        return await db.InsertAsync(item);
    }

    public async Task<int> DeleteTelefonoAsync(Telefono item)
    {
        var db = await _database.GetConnectionAsync();
        return await db.DeleteAsync(item);
    }

    public async Task<int> DeleteTelefonosByClienteIdAsync(int clienteId)
    {
        var db = await _database.GetConnectionAsync();
        var telefonos = await db.Table<Telefono>().Where(t => t.ClienteId == clienteId).ToListAsync();
        int deleted = 0;
        foreach (var tel in telefonos)
        {
            deleted += await db.DeleteAsync(tel);
        }
        return deleted;
    }
}

