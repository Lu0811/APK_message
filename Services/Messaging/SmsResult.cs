using System;

namespace DebtMessageManager.Services.Messaging;

public class SmsResult
{
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.Now;

    public static SmsResult Success() => new() { IsSuccess = true };
    public static SmsResult Failure(string message) => new() { IsSuccess = false, ErrorMessage = message };
}

