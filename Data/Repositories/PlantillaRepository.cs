using DebtMessageManager.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DebtMessageManager.Data.Repositories;

public class PlantillaRepository
{
    private readonly AppDatabase _database;

    public PlantillaRepository(AppDatabase database)
    {
        _database = database;
    }

    public async Task<List<PlantillaMensaje>> GetPlantillasAsync()
    {
        var db = await _database.GetConnectionAsync();
        return await db.Table<PlantillaMensaje>().ToListAsync();
    }

    public async Task<List<PlantillaMensaje>> GetPlantillasActivasAsync()
    {
        var db = await _database.GetConnectionAsync();
        return await db.Table<PlantillaMensaje>().Where(p => p.Activa).ToListAsync();
    }

    public async Task<PlantillaMensaje?> GetPlantillaAsync(int id)
    {
        var db = await _database.GetConnectionAsync();
        return await db.Table<PlantillaMensaje>().Where(p => p.Id == id).FirstOrDefaultAsync();
    }

    public async Task<int> SavePlantillaAsync(PlantillaMensaje item)
    {
        var db = await _database.GetConnectionAsync();
        if (item.Id != 0)
        {
            await db.UpdateAsync(item);
            return item.Id;
        }

        return await db.InsertAsync(item);
    }

    public async Task<int> DeletePlantillaAsync(PlantillaMensaje item)
    {
        var db = await _database.GetConnectionAsync();
        return await db.DeleteAsync(item);
    }
}

