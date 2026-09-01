using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;

namespace DebtMessageManager.Services.Csv;

public class CsvService : ICsvService
{
    private static CsvConfiguration GetConfiguration(Stream stream)
    {
        // Detectar si el delimitador es ';' o ',' leyendo las primeras líneas
        string delimiter = ",";
        long originalPos = stream.Position;
        try
        {
            using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true);
            string? firstLine = reader.ReadLine();
            if (firstLine is not null && firstLine.Contains(';') && !firstLine.Contains(','))
            {
                delimiter = ";";
            }
        }
        finally
        {
            stream.Position = originalPos;
        }

        return new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = delimiter,
            MissingFieldFound = null,
            BadDataFound = null,
            TrimOptions = TrimOptions.Trim,
            HeaderValidated = null
        };
    }

    public List<string> GetHeaders(Stream stream)
    {
        if (stream.CanSeek)
            stream.Position = 0;

        var config = GetConfiguration(stream);
        using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
        using var csv = new CsvReader(reader, config);

        csv.Read();
        csv.ReadHeader();
        return csv.HeaderRecord?.Where(h => !string.IsNullOrWhiteSpace(h)).ToList() ?? new List<string>();
    }

    public async Task<List<Dictionary<string, string>>> ReadCsvAsync(Stream stream)
    {
        if (stream.CanSeek)
            stream.Position = 0;

        var config = GetConfiguration(stream);
        var records = new List<Dictionary<string, string>>();

        using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
        using var csv = new CsvReader(reader, config);

        if (!await csv.ReadAsync())
            return records;

        csv.ReadHeader();
        var headers = csv.HeaderRecord?.Where(h => !string.IsNullOrWhiteSpace(h)).ToArray();
        if (headers is null || headers.Length == 0)
            return records;

        while (await csv.ReadAsync())
        {
            var record = new Dictionary<string, string>();
            foreach (var header in headers)
            {
                record[header] = csv.GetField(header) ?? string.Empty;
            }
            records.Add(record);
        }

        return records;
    }
}

