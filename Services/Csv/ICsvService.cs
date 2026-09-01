using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DebtMessageManager.Services.Csv
{
    public interface ICsvService
    {
        Task<List<Dictionary<string, string>>> ReadCsvAsync(Stream stream);
        List<string> GetHeaders(Stream stream);
    }
}
