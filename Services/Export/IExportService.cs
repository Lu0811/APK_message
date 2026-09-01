using System.IO;
using System.Threading.Tasks;

namespace DebtMessageManager.Services.Export;

public interface IExportService
{
    Task<string> ExportClientsToCsvAsync();
}

