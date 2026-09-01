using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DebtMessageManager.Services.DataSources;

public interface IDataSourceService
{
    string SourceName { get; }
    Task<List<Dictionary<string, string>>> FetchDataAsync(Stream stream);
}

