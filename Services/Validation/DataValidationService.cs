using DebtMessageManager.Helpers;
using DebtMessageManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DebtMessageManager.Services.Validation;

public class DataValidationService : IDataValidationService
{
    public ValidationResult ValidateAndParseRow(Dictionary<string, string> mappedRow)
    {
        var result = new ValidationResult { IsValid = true, ClienteInfo = new Cliente(), TelefonosInfo = new List<Telefono>() };

        // 1. Validar Nombre
        if (mappedRow.TryGetValue("Nombre", out var nombre) && !string.IsNullOrWhiteSpace(nombre))
        {
            result.ClienteInfo.Nombre = nombre.Trim();
        }
        else
        {
            result.IsValid = false;
            result.Errors.Add("Nombre de cliente inválido o vacío.");
        }

        // 2. Validar Monto de Deuda
        if (mappedRow.TryGetValue("Monto", out var montoStr) && CurrencyHelper.TryParse(montoStr, out decimal monto))
        {
            result.ClienteInfo.MontoDeuda = monto;
        }
        else
        {
            result.IsValid = false;
            result.Errors.Add($"Monto de deuda inválido o no numérico ({montoStr}).");
        }

        // 3. Validar Fecha de Vencimiento
        if (mappedRow.TryGetValue("FechaVencimiento", out var fechaStr) && DateHelper.TryParse(fechaStr, out DateTime fechaVencimiento))
        {
            result.ClienteInfo.FechaVencimiento = fechaVencimiento;
        }
        else
        {
            result.IsValid = false;
            result.Errors.Add($"Fecha de vencimiento no reconocida o inválida ({fechaStr}).");
        }

        // 4. Validar y Normalizar Teléfonos
        if (mappedRow.TryGetValue("Telefonos", out var telefonosStr) && !string.IsNullOrWhiteSpace(telefonosStr))
        {
            var numeros = PhoneNumberHelper.ExtractPhoneNumbers(telefonosStr);
            if (numeros.Count > 0)
            {
                foreach (var num in numeros)
                {
                    result.TelefonosInfo.Add(new Telefono { Numero = num, Activo = true });
                }
            }
            else
            {
                result.IsValid = false;
                result.Errors.Add($"Teléfonos inválidos en registro ({telefonosStr}).");
            }
        }
        else
        {
            result.IsValid = false;
            result.Errors.Add("Columna de teléfonos faltante o vacía.");
        }

        result.ClienteInfo.FechaImportacion = DateTime.Now;

        return result;
    }
}

