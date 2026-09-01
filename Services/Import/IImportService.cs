using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DebtMessageManager.Services.Import;

public class ImportPreviewResult
{
    public int TotalRecords { get; set; }
    public int ValidRecords { get; set; }
    public int InvalidRecords { get; set; }
    public List<string> Errors { get; set; } = new();
}

public interface IImportService
{
    Task<ImportPreviewResult> ValidateFileAsync(Stream fileStream, Dictionary<string, string> columnMapping);
    Task<ImportPreviewResult> ProcessAndImportAsync(Stream fileStream, Dictionary<string, string> columnMapping);
}

