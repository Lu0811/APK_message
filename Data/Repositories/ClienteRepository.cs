using DebtMessageManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DebtMessageManager.Data.Repositories;

public class ClienteRepository
{
    private readonly AppDatabase _database;
    private readonly TelefonoRepository _telefonoRepository;

    public ClienteRepository(AppDatabase database, TelefonoRepository telefonoRepository)
    {
        _database = database;
        _telefonoRepository = telefonoRepository;
    }

    public async Task<List<Cliente>> GetClientesAsync(bool includeTelefonos = true)
    {
        var db = await _database.GetConnectionAsync();
        var clientes = await db.Table<Cliente>().ToListAsync();

        if (includeTelefonos)
        {
            var todosTelefonos = await _telefonoRepository.GetAllTelefonosAsync();
            var telLookup = todosTelefonos.GroupBy(t => t.ClienteId).ToDictionary(g => g.Key, g => g.ToList());

            foreach (var cliente in clientes)
            {
                if (telLookup.TryGetValue(cliente.Id, out var tels))
                {
                    cliente.Telefonos = tels;
                }
            }
        }

        return clientes;
    }

    public async Task<Cliente?> GetClienteAsync(int id, bool includeTelefonos = true)
    {
        var db = await _database.GetConnectionAsync();
        var cliente = await db.Table<Cliente>().Where(i => i.Id == id).FirstOrDefaultAsync();

        if (cliente is not null && includeTelefonos)
        {
            cliente.Telefonos = await _telefonoRepository.GetTelefonosByClienteIdAsync(id);
        }

        return cliente;
    }

    public async Task<int> SaveClienteAsync(Cliente item)
    {
        var db = await _database.GetConnectionAsync();
        if (item.Id != 0)
        {
            await db.UpdateAsync(item);
            return item.Id;
        }

        return await db.InsertAsync(item);
    }

    public async Task<int> DeleteClienteAsync(Cliente item)
    {
        var db = await _database.GetConnectionAsync();
        await _telefonoRepository.DeleteTelefonosByClienteIdAsync(item.Id);
        return await db.DeleteAsync(item);
    }

    public async Task<int> DeleteAllClientesAsync()
    {
        var db = await _database.GetConnectionAsync();
        await db.DeleteAllAsync<Telefono>();
        return await db.DeleteAllAsync<Cliente>();
    }
}

