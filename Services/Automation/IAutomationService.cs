using System.Threading.Tasks;

namespace DebtMessageManager.Services.Automation;

public interface IAutomationService
{
    Task<AutomationPreviewResult> EvaluateCampaignAsync();
    Task<AutomationExecutionResult> ExecuteCampaignAsync(AutomationPreviewResult preview);
    Task<AutomationExecutionResult> RetryFailedMessagesAsync();
    bool IsWithinOperatingHours(TimeSpan currentTime, TimeSpan startTime, TimeSpan endTime);
}

