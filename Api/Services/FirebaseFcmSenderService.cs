using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;

namespace sc_manager.Services;

public class FirebaseFcmSenderService
{
    private static bool _appInitialized;
    private static readonly object _lock = new();

    public FirebaseFcmSenderService(IConfiguration configuration)
    {
        if (!_appInitialized)
        {
            lock (_lock)
            {
                if (!_appInitialized)
                {
                    var projectId = configuration["Firebase:ProjectId"] ?? throw new InvalidOperationException("Firebase:ProjectId ist nicht konfiguriert.");

                    FirebaseApp.Create(new AppOptions
                    {
                        Credential = GoogleCredential.GetApplicationDefault(),
                        ProjectId = projectId
                    });
                    _appInitialized = true;
                }
            }
        }
    }

    public async Task<string> SendToTokenAsync(
        string fcmToken,
        string title,
        string body,
        Dictionary<string, string>? data = null)
    {
        var message = new Message
        {
            Token = fcmToken,
            Notification = new Notification
            {
                Title = title,
                Body = body
            },
            Data = data // optional custom payload, e.g. for deep-linking
        };

        try
        {
            // Returns the FCM message ID on success
            return await FirebaseMessaging.DefaultInstance.SendAsync(message);
        }
        catch (FirebaseMessagingException ex)
        {
            throw new ApplicationException($"FCM send failed: {ex.MessagingErrorCode} - {ex.Message}, Token: {fcmToken}", ex);
        }
    }
}