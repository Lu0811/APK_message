using DebtMessageManager.Helpers;
using DebtMessageManager.Models;
using System;
using System.Collections.Generic;

namespace DebtMessageManager.Services.Templates;

public class MessageTemplateService : IMessageTemplateService
{
    public string GenerateMessage(string templateContent, Cliente cliente, int diasRetraso = 0)
    {
        if (string.IsNullOrWhiteSpace(templateContent))
            return string.Empty;

        int retrasoFinal = diasRetraso > 0 ? diasRetraso : cliente.DiasRetraso;

        return templateContent
            .Replace("{NOMBRE}", cliente.Nombre, StringComparison.OrdinalIgnoreCase)
            .Replace("{MONTO}", CurrencyHelper.FormatCurrency(cliente.MontoDeuda), StringComparison.OrdinalIgnoreCase)
            .Replace("{FECHA_VENCIMIENTO}", DateHelper.FormatShortDate(cliente.FechaVencimiento), StringComparison.OrdinalIgnoreCase)
            .Replace("{DIAS_RETRASO}", retrasoFinal.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    public List<string> GetAvailableVariables()
    {
        return new List<string>
        {
            "{NOMBRE}",
            "{MONTO}",
            "{FECHA_VENCIMIENTO}",
            "{DIAS_RETRASO}"
        };
    }
}

