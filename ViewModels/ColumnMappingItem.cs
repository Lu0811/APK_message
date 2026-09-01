namespace DebtMessageManager.ViewModels;

public class ColumnMappingItem
{
    public string CampoInterno { get; set; } = string.Empty;

    public string EncabezadoCsv { get; set; } = string.Empty;

    public override string ToString() => $"{CampoInterno} -> {EncabezadoCsv}";
}