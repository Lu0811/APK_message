using DebtMessageManager.Services.Csv;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DebtMessageManager.Services.DataSources;

public class CsvDataSourceService : IDataSourceService
{
    private readonly ICsvService _csvService;

    public CsvDataSourceService(ICsvService csvService)
    {
        _csvService = csvService;
    }

    public string SourceName => "Archivo CSV Local";

    public async Task<List<Dictionary<string, string>>> FetchDataAsync(Stream stream)
    {
        return await _csvService.ReadCsvAsync(stream);
    }
}

