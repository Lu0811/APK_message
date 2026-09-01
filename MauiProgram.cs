using DebtMessageManager.Data;
using DebtMessageManager.Data.Repositories;
using DebtMessageManager.Services.Automation;
using DebtMessageManager.Services.Csv;
using DebtMessageManager.Services.DataSources;
using DebtMessageManager.Services.Export;
using DebtMessageManager.Services.Import;
using DebtMessageManager.Services.Messaging;
using DebtMessageManager.Services.Templates;
using DebtMessageManager.Services.Validation;
using DebtMessageManager.ViewModels;
using DebtMessageManager.Views;
using Microsoft.Extensions.Logging;

namespace DebtMessageManager;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // 1. Persistencia y Repositorios SQLite
        builder.Services.AddSingleton<AppDatabase>();
        builder.Services.AddSingleton<DatabaseInitializer>();
        builder.Services.AddSingleton<ClienteRepository>();
        builder.Services.AddSingleton<TelefonoRepository>();
        builder.Services.AddSingleton<MensajeRepository>();
        builder.Services.AddSingleton<PlantillaRepository>();
        builder.Services.AddSingleton<ReglaRepository>();
        builder.Services.AddSingleton<ConfiguracionRepository>();

        // 2. Servicios de Lógica de Negocio
        builder.Services.AddSingleton<ICsvService, CsvService>();
        builder.Services.AddSingleton<IDataValidationService, DataValidationService>();
        builder.Services.AddSingleton<IImportService, ImportService>();
        builder.Services.AddSingleton<IMessageTemplateService, MessageTemplateService>();
        builder.Services.AddSingleton<ISmsService, SmsService>();
        builder.Services.AddSingleton<IAutomationService, AutomationService>();
        builder.Services.AddSingleton<IDataSourceService, CsvDataSourceService>();
        builder.Services.AddSingleton<IExportService, ExportService>();

        // 3. ViewModels
        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddSingleton<DashboardViewModel>();
        builder.Services.AddSingleton<ImportViewModel>();
        builder.Services.AddSingleton<ClientesViewModel>();
        builder.Services.AddSingleton<ClienteDetalleViewModel>();
        builder.Services.AddSingleton<MensajesViewModel>();
        builder.Services.AddSingleton<ConfiguracionViewModel>();

        // 4. Views
        builder.Services.AddSingleton<DashboardPage>();
        builder.Services.AddSingleton<ImportPage>();
        builder.Services.AddSingleton<ClientesPage>();
        builder.Services.AddTransient<ClienteDetallePage>();
        builder.Services.AddSingleton<MensajesPage>();
        builder.Services.AddSingleton<ConfiguracionPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();
        App.Services = app.Services;
        return app;
    }
}

