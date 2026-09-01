using DebtMessageManager.Models;
using System;
using System.Threading.Tasks;

namespace DebtMessageManager.Data.Repositories;

public class ConfiguracionRepository
{
    private readonly AppDatabase _database;

    public ConfiguracionRepository(AppDatabase database)
    {
        _database = database;
    }

    public async Task<ConfiguracionAutomatizacion> GetConfiguracionAsync()
    {
        var db = await _database.GetConnectionAsync();
        var config = await db.Table<ConfiguracionAutomatizacion>().FirstOrDefaultAsync();

        if (config is null)
        {
            config = new ConfiguracionAutomatizacion
            {
                HoraInicio = new TimeSpan(8, 0, 0),
                HoraFin = new TimeSpan(18, 0, 0),
                DiasGracia = 3,
                AutomatizacionActiva = true
            };
            config.Id = await db.InsertAsync(config);
        }

        return config;
    }

    public async Task<int> SaveConfiguracionAsync(ConfiguracionAutomatizacion config)
    {
        var db = await _database.GetConnectionAsync();
        if (config.Id != 0)
        {
            return await db.UpdateAsync(config);
        }

        return await db.InsertAsync(config);
    }
}

