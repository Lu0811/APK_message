using DebtMessageManager.Models;
using System;
using System.Threading.Tasks;

namespace DebtMessageManager.Data;

public class DatabaseInitializer
{
    private readonly AppDatabase _database;

    public DatabaseInitializer(AppDatabase database)
    {
        _database = database;
    }

    public async Task InitializeAsync()
    {
        var db = await _database.GetConnectionAsync();

        // 1. Configuración por defecto
        var config = await db.Table<ConfiguracionAutomatizacion>().FirstOrDefaultAsync();
        if (config is null)
        {
            await db.InsertAsync(new ConfiguracionAutomatizacion
            {
                HoraInicio = new TimeSpan(8, 0, 0),
                HoraFin = new TimeSpan(18, 0, 0),
                DiasGracia = 3,
                AutomatizacionActiva = true
            });
        }

        // 2. Plantillas por defecto
        var totalPlantillas = await db.Table<PlantillaMensaje>().CountAsync();
        if (totalPlantillas == 0)
        {
            var p1 = new PlantillaMensaje
            {
                Nombre = "Recordatorio Preventivo",
                Contenido = "Hola {NOMBRE}, te recordamos que tu cuota de {MONTO} vence el {FECHA_VENCIMIENTO}. Realiza tu pago a tiempo y mantén tu buen historial crediticio.",
                Activa = true
            };
            var p2 = new PlantillaMensaje
            {
                Nombre = "Primer Aviso Vencido",
                Contenido = "Estimado(a) {NOMBRE}, te informamos que tu deuda de {MONTO} venció el {FECHA_VENCIMIENTO} ({DIAS_RETRASO} días de mora). Por favor regulariza tu pago.",
                Activa = true
            };
            var p3 = new PlantillaMensaje
            {
                Nombre = "Aviso Urgente de Cobranza",
                Contenido = "URGENTE {NOMBRE}: Tienes {DIAS_RETRASO} días de atraso en tu deuda de {MONTO} vencida el {FECHA_VENCIMIENTO}. Comunícate para evitar reportes negativos.",
                Activa = true
            };

            await db.InsertAsync(p1);
            await db.InsertAsync(p2);
            await db.InsertAsync(p3);
        }

        // 3. Reglas de Envío por defecto
        var totalReglas = await db.Table<ReglaEnvio>().CountAsync();
        if (totalReglas == 0)
        {
            var plantillas = await db.Table<PlantillaMensaje>().ToListAsync();
            int p1Id = plantillas.Count > 0 ? plantillas[0].Id : 1;
            int p2Id = plantillas.Count > 1 ? plantillas[1].Id : p1Id;
            int p3Id = plantillas.Count > 2 ? plantillas[2].Id : p2Id;

            await db.InsertAsync(new ReglaEnvio
            {
                Nombre = "3 días antes del vencimiento",
                Tipo = TipoReglaEnvio.AntesVencimiento.ToString(),
                Dias = 3,
                PlantillaId = p1Id,
                Activa = true
            });

            await db.InsertAsync(new ReglaEnvio
            {
                Nombre = "1 día después del vencimiento",
                Tipo = TipoReglaEnvio.DespuesVencimiento.ToString(),
                Dias = 1,
                PlantillaId = p2Id,
                Activa = true
            });

            await db.InsertAsync(new ReglaEnvio
            {
                Nombre = "7 días después del vencimiento",
                Tipo = TipoReglaEnvio.DespuesVencimiento.ToString(),
                Dias = 7,
                PlantillaId = p2Id,
                Activa = true
            });

            await db.InsertAsync(new ReglaEnvio
            {
                Nombre = "15 días después del vencimiento",
                Tipo = TipoReglaEnvio.DespuesVencimiento.ToString(),
                Dias = 15,
                PlantillaId = p3Id,
                Activa = true
            });
        }
    }
}

