using System.Threading.Tasks;

namespace DebtMessageManager.Services.Messaging;

public interface ISmsService
{
    Task<bool> CheckAndRequestPermissionAsync();
    Task<SmsResult> SendSmsAsync(string phoneNumber, string message);
}

