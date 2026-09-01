using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DebtMessageManager.Data.Repositories;
using DebtMessageManager.Models;
using DebtMessageManager.Services.Templates;
using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace DebtMessageManager.ViewModels;

public partial class ConfiguracionViewModel : ObservableObject
{
    private readonly ConfiguracionRepository _configRepository;
    private readonly ReglaRepository _reglaRepository;
    private readonly PlantillaRepository _plantillaRepository;
    private readonly IMessageTemplateService _templateService;

    [ObservableProperty]
    public partial TimeSpan HoraInicio { get; set; } = new(8, 0, 0);

    [ObservableProperty]
    public partial TimeSpan HoraFin { get; set; } = new(18, 0, 0);

    [ObservableProperty]
    public partial int DiasGracia { get; set; } = 3;

    [ObservableProperty]
    public partial bool AutomatizacionActiva { get; set; } = true;

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = string.Empty;

    // Campos para edición de plantilla
    [ObservableProperty]
    public partial PlantillaMensaje? PlantillaSeleccionada { get; set; }

    [ObservableProperty]
    public partial string PlantillaNombre { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string PlantillaContenido { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string PlantillaVistaPrevia { get; set; } = string.Empty;

    public ObservableCollection<ReglaEnvio> Reglas { get; } = new();
    public ObservableCollection<PlantillaMensaje> Plantillas { get; } = new();

    public ConfiguracionViewModel(
        ConfiguracionRepository configRepository,
        ReglaRepository reglaRepository,
        PlantillaRepository plantillaRepository,
        IMessageTemplateService templateService)
    {
        _configRepository = configRepository;
        _reglaRepository = reglaRepository;
        _plantillaRepository = plantillaRepository;
        _templateService = templateService;
    }

    [RelayCommand]
    public async Task CargarAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var config = await _configRepository.GetConfiguracionAsync();
            HoraInicio = config.HoraInicio;
            HoraFin = config.HoraFin;
            DiasGracia = config.DiasGracia;
            AutomatizacionActiva = config.AutomatizacionActiva;

            var reglasList = await _reglaRepository.GetReglasAsync();
            Reglas.Clear();
            foreach (var r in reglasList)
            {
                Reglas.Add(r);
            }

            var plantillasList = await _plantillaRepository.GetPlantillasAsync();
            Plantillas.Clear();
            foreach (var p in plantillasList)
            {
                Plantillas.Add(p);
            }

            if (Plantillas.Count > 0 && PlantillaSeleccionada is null)
            {
                SeleccionarPlantilla(Plantillas.First());
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error al cargar: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task GuardarConfiguracionAsync()
    {
        IsBusy = true;
        try
        {
            var config = await _configRepository.GetConfiguracionAsync();
            config.HoraInicio = HoraInicio;
            config.HoraFin = HoraFin;
            config.DiasGracia = DiasGracia;
            config.AutomatizacionActiva = AutomatizacionActiva;

            await _configRepository.SaveConfiguracionAsync(config);

            if (Shell.Current is not null)
                await Shell.Current.DisplayAlertAsync("Configuración Guardada", "Los parámetros de automatización se actualizaron correctamente.", "OK");
        }
        catch (Exception ex)
        {
            if (Shell.Current is not null)
                await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task AlternarReglaActivaAsync(ReglaEnvio? regla)
    {
        if (regla is null) return;
        regla.Activa = !regla.Activa;
        await _reglaRepository.SaveReglaAsync(regla);
        await CargarAsync();
    }

    [RelayCommand]
    public void SeleccionarPlantilla(PlantillaMensaje? plantilla)
    {
        if (plantilla is null)
        {
            PlantillaSeleccionada = null;
            PlantillaNombre = string.Empty;
            PlantillaContenido = string.Empty;
            ActualizarVistaPrevia();
            return;
        }

        PlantillaSeleccionada = plantilla;
        PlantillaNombre = plantilla.Nombre;
        PlantillaContenido = plantilla.Contenido;
        ActualizarVistaPrevia();
    }

    partial void OnPlantillaContenidoChanged(string value)
    {
        ActualizarVistaPrevia();
    }

    private void ActualizarVistaPrevia()
    {
        var dummyCliente = new Cliente
        {
            Nombre = "Luciana Huaman",
            MontoDeuda = 520,
            FechaVencimiento = DateTime.Today.AddDays(-6)
        };

        PlantillaVistaPrevia = _templateService.GenerateMessage(PlantillaContenido, dummyCliente, diasRetraso: 6);
    }

    [RelayCommand]
    public async Task GuardarPlantillaAsync()
    {
        if (string.IsNullOrWhiteSpace(PlantillaNombre) || string.IsNullOrWhiteSpace(PlantillaContenido))
        {
            if (Shell.Current is not null)
                await Shell.Current.DisplayAlertAsync("Campos requeridos", "Debe ingresar el nombre y contenido de la plantilla.", "OK");
            return;
        }

        var plantilla = PlantillaSeleccionada ?? new PlantillaMensaje();
        plantilla.Nombre = PlantillaNombre.Trim();
        plantilla.Contenido = PlantillaContenido.Trim();
        plantilla.Activa = true;

        await _plantillaRepository.SavePlantillaAsync(plantilla);

        if (Shell.Current is not null)
            await Shell.Current.DisplayAlertAsync("Plantilla Guardada", "La plantilla de mensaje se guardó correctamente.", "OK");

        await CargarAsync();
    }

    [RelayCommand]
    public void InsertarVariable(string variable)
    {
        PlantillaContenido += $" {variable} ";
    }
}

