using System;
using System.Globalization;

namespace DebtMessageManager.Helpers;

public static class DateHelper
{
    private static readonly CultureInfo SpanishCulture = CultureInfo.GetCultureInfo("es-PE");
    private static readonly string[] CommonFormats = 
    { 
        "dd/MM/yyyy", "d/M/yyyy", "dd-MM-yyyy", "d-M-yyyy", 
        "yyyy-MM-dd", "yyyy/MM/dd", "dd.MM.yyyy", "d.M.yyyy" 
    };

    public static bool TryParse(string? input, out DateTime date)
    {
        date = DateTime.MinValue;
        if (string.IsNullOrWhiteSpace(input))
            return false;

        var trimmed = input.Trim();

        return DateTime.TryParseExact(trimmed, CommonFormats, SpanishCulture, DateTimeStyles.None, out date)
            || DateTime.TryParseExact(trimmed, CommonFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out date)
            || DateTime.TryParse(trimmed, SpanishCulture, DateTimeStyles.None, out date)
            || DateTime.TryParse(trimmed, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
    }

    public static string FormatShortDate(DateTime date)
    {
        return date.ToString("dd/MM/yyyy");
    }

    public static string FormatDateTime(DateTime date)
    {
        return date.ToString("dd/MM/yyyy HH:mm");
    }
}

