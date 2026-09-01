using DebtMessageManager.Data.Repositories;
using DebtMessageManager.Helpers;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace DebtMessageManager.Services.Export;

public class ExportService : IExportService
{
    private readonly ClienteRepository _clienteRepository;
    private readonly MensajeRepository _mensajeRepository;

    public ExportService(ClienteRepository clienteRepository, MensajeRepository mensajeRepository)
    {
        _clienteRepository = clienteRepository;
        _mensajeRepository = mensajeRepository;
    }

    public async Task<string> ExportClientsToCsvAsync()
    {
        var clientes = await _clienteRepository.GetClientesAsync(includeTelefonos: true);
        var todosMensajes = await _mensajeRepository.GetMensajesAsync();
        var mensajesPorCliente = todosMensajes.GroupBy(m => m.ClienteId).ToDictionary(g => g.Key, g => g.OrderByDescending(m => m.FechaProgramada).ToList());

        var sb = new StringBuilder();
        sb.AppendLine("ID,NOMBRE,MONTO_DEUDA,TELEFONOS,FECHA_VENCIMIENTO,ESTADO_DEUDA,ESTADO_MENSAJE,ULTIMO_CONTACTO");

        foreach (var c in clientes)
        {
            string telefonos = string.Join(" / ", c.Telefonos.Select(t => t.Numero));
            string estadoMensaje = "Sin Envíos";
            string ultimoContacto = "N/A";

            if (mensajesPorCliente.TryGetValue(c.Id, out var msgs) && msgs.Count > 0)
            {
                var ult = msgs.First();
                estadoMensaje = ult.Estado;
                ultimoContacto = ult.FechaMostrar;
            }

            sb.AppendLine($"\"{c.Id}\",\"{c.Nombre}\",\"{c.MontoDeuda:F2}\",\"{telefonos}\",\"{DateHelper.FormatShortDate(c.FechaVencimiento)}\",\"{c.EstadoTexto}\",\"{estadoMensaje}\",\"{ultimoContacto}\"");
        }

        string fileName = $"Cobranzas_Export_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        string filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);
        await File.WriteAllTextAsync(filePath, sb.ToString(), Encoding.UTF8);

        return filePath;
    }
}

