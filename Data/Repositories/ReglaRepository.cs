using DebtMessageManager.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DebtMessageManager.Data.Repositories;

public class ReglaRepository
{
    private readonly AppDatabase _database;

    public ReglaRepository(AppDatabase database)
    {
        _database = database;
    }

    public async Task<List<ReglaEnvio>> GetReglasAsync()
    {
        var db = await _database.GetConnectionAsync();
        return await db.Table<ReglaEnvio>().ToListAsync();
    }

    public async Task<List<ReglaEnvio>> GetReglasActivasAsync()
    {
        var db = await _database.GetConnectionAsync();
        return await db.Table<ReglaEnvio>().Where(r => r.Activa).ToListAsync();
    }

    public async Task<ReglaEnvio?> GetReglaAsync(int id)
    {
        var db = await _database.GetConnectionAsync();
        return await db.Table<ReglaEnvio>().Where(r => r.Id == id).FirstOrDefaultAsync();
    }

    public async Task<int> SaveReglaAsync(ReglaEnvio item)
    {
        var db = await _database.GetConnectionAsync();
        if (item.Id != 0)
        {
            await db.UpdateAsync(item);
            return item.Id;
        }

        return await db.InsertAsync(item);
    }

    public async Task<int> DeleteReglaAsync(ReglaEnvio item)
    {
        var db = await _database.GetConnectionAsync();
        return await db.DeleteAsync(item);
    }
}

