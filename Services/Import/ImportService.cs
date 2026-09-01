using DebtMessageManager.Data.Repositories;
using DebtMessageManager.Services.Csv;
using DebtMessageManager.Services.Validation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DebtMessageManager.Services.Import;

public class ImportService : IImportService
{
    private readonly ICsvService _csvService;
    private readonly IDataValidationService _validationService;
    private readonly ClienteRepository _clienteRepository;
    private readonly TelefonoRepository _telefonoRepository;

    public ImportService(
        ICsvService csvService,
        IDataValidationService validationService,
        ClienteRepository clienteRepository,
        TelefonoRepository telefonoRepository)
    {
        _csvService = csvService;
        _validationService = validationService;
        _clienteRepository = clienteRepository;
        _telefonoRepository = telefonoRepository;
    }

    public async Task<ImportPreviewResult> ValidateFileAsync(Stream fileStream, Dictionary<string, string> columnMapping)
    {
        var result = new ImportPreviewResult();
        var rawData = await _csvService.ReadCsvAsync(fileStream);
        var mapping = columnMapping ?? new Dictionary<string, string>();

        result.TotalRecords = rawData.Count;
        int rowIndex = 1;

        foreach (var row in rawData)
        {
            rowIndex++;
            var mappedRow = new Dictionary<string, string>();
            foreach (var map in mapping)
            {
                if (!string.IsNullOrWhiteSpace(map.Value) && row.TryGetValue(map.Value, out var csvValue))
                {
                    mappedRow[map.Key] = csvValue?.Trim() ?? string.Empty;
                }
            }

            var validation = _validationService.ValidateAndParseRow(mappedRow);
            if (validation.IsValid)
            {
                result.ValidRecords++;
            }
            else
            {
                result.InvalidRecords++;
                foreach (var err in validation.Errors)
                {
                    result.Errors.Add($"Fila {rowIndex}: {err}");
                }
            }
        }

        return result;
    }

    public async Task<ImportPreviewResult> ProcessAndImportAsync(Stream fileStream, Dictionary<string, string> columnMapping)
    {
        var result = new ImportPreviewResult();
        var rawData = await _csvService.ReadCsvAsync(fileStream);
        var mapping = columnMapping ?? new Dictionary<string, string>();

        result.TotalRecords = rawData.Count;
        int rowIndex = 1;

        foreach (var row in rawData)
        {
            rowIndex++;
            var mappedRow = new Dictionary<string, string>();

            foreach (var map in mapping)
            {
                if (!string.IsNullOrWhiteSpace(map.Value) && row.TryGetValue(map.Value, out var csvValue))
                {
                    mappedRow[map.Key] = csvValue?.Trim() ?? string.Empty;
                }
            }

            var validation = _validationService.ValidateAndParseRow(mappedRow);

            if (validation.IsValid)
            {
                var clienteId = await _clienteRepository.SaveClienteAsync(validation.ClienteInfo);
                validation.ClienteInfo.Id = clienteId;

                foreach (var tel in validation.TelefonosInfo)
                {
                    tel.ClienteId = clienteId;
                    await _telefonoRepository.SaveTelefonoAsync(tel);
                }

                result.ValidRecords++;
            }
            else
            {
                result.InvalidRecords++;
                foreach (var err in validation.Errors)
                {
                    result.Errors.Add($"Fila {rowIndex}: {err}");
                }
            }
        }

        return result;
    }
}