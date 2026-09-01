using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DebtMessageManager.Services.Csv;
using DebtMessageManager.Services.Import;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DebtMessageManager.ViewModels;

public partial class ImportViewModel : ObservableObject
{
    private readonly ICsvService _csvService;
    private readonly IImportService _importService;
    private FileResult? _selectedFile;

    [ObservableProperty]
    public partial string EstadoImportacion { get; set; } = "Ningún archivo seleccionado";

    [ObservableProperty]
    public partial string ArchivoSeleccionado { get; set; } = "No seleccionado";

    [ObservableProperty]
    public partial int TotalRegistros { get; set; }

    [ObservableProperty]
    public partial int RegistrosValidos { get; set; }

    [ObservableProperty]
    public partial int RegistrosInvalidos { get; set; }

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial bool TieneArchivo { get; set; }

    [ObservableProperty]
    public partial bool TieneErrores { get; set; }

    public ObservableCollection<string> EncabezadosDetectados { get; } = new();
    public ObservableCollection<ColumnMappingItem> MapeoColumnas { get; } = new();
    public ObservableCollection<ImportPreviewItem> VistaPrevia { get; } = new();
    public ObservableCollection<string> ListaErrores { get; } = new();

    public ImportViewModel(ICsvService csvService, IImportService importService)
    {
        _csvService = csvService;
        _importService = importService;
    }

    [RelayCommand]
    public async Task SeleccionarArchivoAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Seleccionar archivo CSV de cobranzas",
                FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.WinUI, new[] { ".csv", ".txt" } },
                    { DevicePlatform.Android, new[] { "text/csv", "text/comma-separated-values", "application/csv", "application/vnd.ms-excel", "text/plain" } },
                    { DevicePlatform.iOS, new[] { "public.comma-separated-values-text", "public.plain-text" } },
                    { DevicePlatform.MacCatalyst, new[] { "public.comma-separated-values-text", "public.plain-text" } }
                })
            });

            if (result is null)
            {
                EstadoImportacion = "Selección cancelada";
                return;
            }

            _selectedFile = result;
            ArchivoSeleccionado = result.FileName;
            TieneArchivo = true;

            using var stream = await result.OpenReadAsync();
            var headers = _csvService.GetHeaders(stream);

            EncabezadosDetectados.Clear();
            foreach (var header in headers)
            {
                EncabezadosDetectados.Add(header);
            }

            MapeoColumnas.Clear();
            MapeoColumnas.Add(new ColumnMappingItem { CampoInterno = "Nombre", EncabezadoCsv = DetectHeader(headers, "nombre", "cliente", "titular", "persona") });
            MapeoColumnas.Add(new ColumnMappingItem { CampoInterno = "Monto", EncabezadoCsv = DetectHeader(headers, "monto", "deuda", "saldo", "importe", "total") });
            MapeoColumnas.Add(new ColumnMappingItem { CampoInterno = "Telefonos", EncabezadoCsv = DetectHeader(headers, "telefono", "telefonos", "celular", "celulares", "movil", "fono") });
            MapeoColumnas.Add(new ColumnMappingItem { CampoInterno = "FechaVencimiento", EncabezadoCsv = DetectHeader(headers, "vencimiento", "fecha", "venc", "limite", "f_venc") });

            VistaPrevia.Clear();
            var rows = await _csvService.ReadCsvAsync(stream);
            foreach (var row in rows.Take(5))
            {
                var preview = new ImportPreviewItem();
                foreach (var pair in row)
                {
                    preview.Valores[pair.Key] = pair.Value;
                }
                VistaPrevia.Add(preview);
            }

            // Ejecutar prevalidación automática
            await ValidarArchivoAsync();
            EstadoImportacion = $"Archivo cargado: {result.FileName}";
        }
        catch (Exception ex)
        {
            EstadoImportacion = $"Error al leer archivo: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task ValidarArchivoAsync()
    {
        if (_selectedFile is null) return;

        try
        {
            using var stream = await _selectedFile.OpenReadAsync();
            var mapping = GetCurrentMapping();

            var previewResult = await _importService.ValidateFileAsync(stream, mapping);
            TotalRegistros = previewResult.TotalRecords;
            RegistrosValidos = previewResult.ValidRecords;
            RegistrosInvalidos = previewResult.InvalidRecords;

            ListaErrores.Clear();
            foreach (var err in previewResult.Errors.Take(30))
            {
                ListaErrores.Add(err);
            }
            TieneErrores = ListaErrores.Count > 0;
        }
        catch (Exception ex)
        {
            EstadoImportacion = $"Error en validación: {ex.Message}";
        }
    }

    [RelayCommand]
    public async Task ImportarAsync()
    {
        if (_selectedFile is null)
        {
            EstadoImportacion = "Primero selecciona un archivo CSV";
            return;
        }

        if (IsBusy) return;
        IsBusy = true;

        try
        {
            using var stream = await _selectedFile.OpenReadAsync();
            var mapping = GetCurrentMapping();

            var result = await _importService.ProcessAndImportAsync(stream, mapping);

            TotalRegistros = result.TotalRecords;
            RegistrosValidos = result.ValidRecords;
            RegistrosInvalidos = result.InvalidRecords;

            ListaErrores.Clear();
            foreach (var err in result.Errors.Take(30))
            {
                ListaErrores.Add(err);
            }
            TieneErrores = ListaErrores.Count > 0;

            EstadoImportacion = $"¡Importación exitosa! {result.ValidRecords} guardados, {result.InvalidRecords} omitidos.";

            if (Shell.Current is not null)
            {
                await Shell.Current.DisplayAlertAsync(
                    "Importación Finalizada",
                    $"Se importaron {result.ValidRecords} registros válidos a SQLite.\nRegistros con errores: {result.InvalidRecords}",
                    "OK");
            }
        }
        catch (Exception ex)
        {
            EstadoImportacion = $"Error durante importación: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private Dictionary<string, string> GetCurrentMapping()
    {
        return MapeoColumnas
            .Where(m => !string.IsNullOrWhiteSpace(m.CampoInterno) && !string.IsNullOrWhiteSpace(m.EncabezadoCsv))
            .ToDictionary(m => m.CampoInterno, m => m.EncabezadoCsv);
    }

    private static string DetectHeader(IEnumerable<string> headers, params string[] keywords)
    {
        foreach (var header in headers)
        {
            if (keywords.Any(keyword => header.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
            {
                return header;
            }
        }

        return headers.FirstOrDefault() ?? string.Empty;
    }
}

