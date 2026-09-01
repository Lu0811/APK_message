using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DebtMessageManager.Helpers;

public static class PhoneNumberHelper
{
    public static List<string> ExtractPhoneNumbers(string? rawInput)
    {
        var results = new List<string>();
        if (string.IsNullOrWhiteSpace(rawInput)) 
            return results;

        // Separar posibles múltiples teléfonos delimitados por / , ; o guiones largos / palabras
        var splits = rawInput.Split(new[] { '/', ',', ';', '|', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var split in splits)
        {
            var cleaned = NormalizePhoneNumber(split);
            if (IsValidPhoneNumber(cleaned) && !results.Contains(cleaned))
            {
                results.Add(cleaned);
            }
        }

        return results;
    }

    public static string NormalizePhoneNumber(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Conservar signo + si está al inicio, eliminar espacios, guiones, puntos y paréntesis
        string trimmed = input.Trim();
        bool hasPlus = trimmed.StartsWith("+");
        string digitsOnly = Regex.Replace(trimmed, @"\D", "");

        if (hasPlus && digitsOnly.Length > 0)
        {
            return "+" + digitsOnly;
        }

        return digitsOnly;
    }

    public static bool IsValidPhoneNumber(string number)
    {
        if (string.IsNullOrWhiteSpace(number))
            return false;

        var digitsOnly = Regex.Replace(number, @"\D", "");
        // Teléfonos celulares en Perú tienen 9 dígitos (o con código de país +51 = 11 dígitos), 
        // o entre 7 y 15 dígitos en estándar internacional.
        return digitsOnly.Length >= 7 && digitsOnly.Length <= 15;
    }
}

