using System;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.ApplicationModel.Communication;

#if ANDROID
using Android.App;
using Android.Content;
using Android.OS;
using Android.Telephony;
#endif

namespace DebtMessageManager.Services.Messaging;

public class SmsService : ISmsService
{
    public async Task<bool> CheckAndRequestPermissionAsync()
    {
#if ANDROID
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.Sms>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.Sms>();
            }
            return status == PermissionStatus.Granted;
        }
        catch (Exception)
        {
            return false;
        }
#else
        return await Task.FromResult(true);
#endif
    }

    public async Task<SmsResult> SendSmsAsync(string phoneNumber, string message)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return SmsResult.Failure("Número de teléfono vacío o no válido.");

        if (string.IsNullOrWhiteSpace(message))
            return SmsResult.Failure("El contenido del mensaje está vacío.");

#if ANDROID
        try
        {
            var hasPermission = await CheckAndRequestPermissionAsync();
            if (!hasPermission)
            {
                return SmsResult.Failure("Permiso SEND_SMS denegado por el usuario.");
            }

            SmsManager? smsManager = null;
#pragma warning disable CA1416, CA1422
            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                var context = Android.App.Application.Context;
                smsManager = context.GetSystemService(Java.Lang.Class.FromType(typeof(SmsManager))) as SmsManager;
            }

            smsManager ??= SmsManager.Default;
#pragma warning restore CA1416, CA1422

            if (smsManager is null)
            {
                return SmsResult.Failure("No se pudo obtener el gestor SMS de Android.");
            }

            var parts = smsManager.DivideMessage(message);
            if (parts is not null && parts.Count > 1)
            {
                smsManager.SendMultipartTextMessage(phoneNumber, null, parts, null, null);
            }
            else
            {
                smsManager.SendTextMessage(phoneNumber, null, message, null, null);
            }

            return SmsResult.Success();
        }
        catch (Exception ex)
        {
            return SmsResult.Failure($"Error al enviar SMS: {ex.Message}");
        }
#elif IOS || MACCATALYST
        try
        {
            if (Sms.Default.IsComposeSupported)
            {
                var smsMessage = new SmsMessage(message, phoneNumber);
                await Sms.Default.ComposeAsync(smsMessage);
                return SmsResult.Success();
            }
            return SmsResult.Failure("La composición de SMS no está soportada en este dispositivo.");
        }
        catch (Exception ex)
        {
            return SmsResult.Failure($"Error al abrir compositor SMS: {ex.Message}");
        }
#else
        // Simulación en Windows / Entorno de desarrollo
        await Task.Delay(100);
        return SmsResult.Success();
#endif
    }
}

