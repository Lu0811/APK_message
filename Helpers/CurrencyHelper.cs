using System.Globalization;

namespace DebtMessageManager.Helpers;

public static class CurrencyHelper
{
    private static readonly CultureInfo SpanishCulture = CultureInfo.GetCultureInfo("es-PE");

    public static bool TryParse(string? input, out decimal amount)
    {
        amount = 0;
        if (string.IsNullOrWhiteSpace(input))
            return false;

        var cleaned = input.Trim()
            .Replace("S/", "", System.StringComparison.OrdinalIgnoreCase)
            .Replace("$", "")
            .Replace("€", "")
            .Trim();

        return decimal.TryParse(cleaned, NumberStyles.Any, SpanishCulture, out amount)
            || decimal.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out amount);
    }

    public static string FormatCurrency(decimal amount)
    {
        return $"S/ {amount:N2}";
    }
}

