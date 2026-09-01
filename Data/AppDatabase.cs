using DebtMessageManager.Models;
using SQLite;
using System.IO;
using System.Threading.Tasks;

namespace DebtMessageManager.Data;

public class AppDatabase
{
    private SQLiteAsyncConnection? _database;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public async Task<SQLiteAsyncConnection> GetConnectionAsync()
    {
        if (_database is not null)
            return _database;

        await _semaphore.WaitAsync();
        try
        {
            if (_database is not null)
                return _database;

            var db = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
            await db.CreateTableAsync<Cliente>();
            await db.CreateTableAsync<Telefono>();
            await db.CreateTableAsync<Mensaje>();
            await db.CreateTableAsync<PlantillaMensaje>();
            await db.CreateTableAsync<ReglaEnvio>();
            await db.CreateTableAsync<ConfiguracionAutomatizacion>();

            _database = db;
            return _database;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}

public static class Constants
{
    public const string DatabaseFilename = "DebtMessageManager.db3";

    public const SQLite.SQLiteOpenFlags Flags =
        SQLite.SQLiteOpenFlags.ReadWrite |
        SQLite.SQLiteOpenFlags.Create |
        SQLite.SQLiteOpenFlags.SharedCache;

    public static string DatabasePath =>
        Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);
}

