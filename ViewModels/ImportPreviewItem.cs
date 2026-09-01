using System.Collections.Generic;
using System.Linq;

namespace DebtMessageManager.ViewModels;

public class ImportPreviewItem
{
    public Dictionary<string, string> Valores { get; } = new();

    public override string ToString() => string.Join(" | ", Valores.Select(x => $"{x.Key}: {x.Value}"));
}