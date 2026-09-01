using DebtMessageManager.Models;
using System.Collections.Generic;

namespace DebtMessageManager.Services.Validation;

public class ValidationResult
{
    public bool IsValid { get; set; } = true;
    public Cliente ClienteInfo { get; set; } = new();
    public List<Telefono> TelefonosInfo { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}

public interface IDataValidationService
{
    ValidationResult ValidateAndParseRow(Dictionary<string, string> mappedRow);
}

