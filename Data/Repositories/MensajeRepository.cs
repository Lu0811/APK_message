using DebtMessageManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DebtMessageManager.Data.Repositories;

public class MensajeRepository
{
    private readonly AppDatabase _database;

    public MensajeRepository(AppDatabase database)
    {
        _database = database;
    }

    public async Task<List<Mensaje>> GetMensajesAsync()
    {
        var db = await _database.GetConnectionAsync();
        return await db.Table<Mensaje>().OrderByDescending(m => m.FechaProgramada).ToListAsync();
    }

    public async Task<Mensaje?> GetMensajeAsync(int id)
    {
        var db = await _database.GetConnectionAsync();
        return await db.Table<Mensaje>().Where(m => m.Id == id).FirstOrDefaultAsync();
    }

    public async Task<List<Mensaje>> GetMensajesByClienteIdAsync(int clienteId)
    {
        var db = await _database.GetConnectionAsync();
        return await db.Table<Mensaje>()
            .Where(m => m.ClienteId == clienteId)
            .OrderByDescending(m => m.FechaProgramada)
            .ToListAsync();
    }

    public async Task<List<Mensaje>> GetMensajesByEstadoAsync(EstadoMensaje estado)
    {
        var db = await _database.GetConnectionAsync();
        string estadoStr = estado.ToString();
        return await db.Table<Mensaje>()
            .Where(m => m.Estado == estadoStr)
            .OrderByDescending(m => m.FechaProgramada)
            .ToListAsync();
    }

    /// <summary>
    /// Evita duplicar mensajes para el mismo cliente, misma regla y misma fecha/período.
    /// Si ya existe un mensaje enviado con éxito o en proceso en la misma fecha (o mes para reglas periódicas), retorna true.
    /// </summary>
    public async Task<bool> ExisteMensajeEnviadoAsync(int clienteId, int reglaId, DateTime fechaReferencia)
    {
        var db = await _database.GetConnectionAsync();
        var inicioDia = fechaReferencia.Date;
        var finDia = inicioDia.AddDays(1);

        string enviadoStr = EstadoMensaje.Enviado.ToString();
        string programadoStr = EstadoMensaje.Programado.ToString();
        string enviandoStr = EstadoMensaje.Enviando.ToString();

        var coincidencias = await db.Table<Mensaje>()
            .Where(m => m.ClienteId == clienteId && 
                        m.ReglaId == reglaId && 
                        (m.Estado == enviadoStr || m.Estado == programadoStr || m.Estado == enviandoStr))
            .ToListAsync();

        // Si ya fue enviado hoy o en la misma fecha de referencia
        return coincidencias.Any(m => m.FechaProgramada.Date == inicioDia || (m.FechaEnvio.HasValue && m.FechaEnvio.Value.Date == inicioDia));
    }

    public async Task<int> SaveMensajeAsync(Mensaje item)
    {
        var db = await _database.GetConnectionAsync();
        if (item.Id != 0)
        {
            await db.UpdateAsync(item);
            return item.Id;
        }

        return await db.InsertAsync(item);
    }

    public async Task<int> DeleteMensajeAsync(Mensaje item)
    {
        var db = await _database.GetConnectionAsync();
        return await db.DeleteAsync(item);
    }

    public async Task<int> ClearMensajesAsync()
    {
        var db = await _database.GetConnectionAsync();
        return await db.DeleteAllAsync<Mensaje>();
    }
}

