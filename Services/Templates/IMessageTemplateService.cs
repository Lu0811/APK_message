using DebtMessageManager.Models;

namespace DebtMessageManager.Services.Templates;

public interface IMessageTemplateService
{
    string GenerateMessage(string templateContent, Cliente cliente, int diasRetraso = 0);
    List<string> GetAvailableVariables();
}

