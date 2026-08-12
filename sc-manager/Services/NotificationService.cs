using Microsoft.JSInterop;

namespace sc_manager.Services;

public class NotificationService : IAsyncDisposable
{
    private readonly IJSRuntime _js;
    private readonly FirebaseConfig _config;
    private IJSObjectReference? _messagingModule;
    private DotNetObjectReference<NotificationService>? _selfRef;
    private bool _initialized;

    public event Action<string?, string?>? OnNotificationReceived;

    public NotificationService(IJSRuntime js, FirebaseConfig config)
    {
        _js = js;
        _config = config;
    }

    private async Task EnsureInitializedAsync()
    {
        if (_initialized) return;

        _initialized = true;
        _messagingModule = await _js.InvokeAsync<IJSObjectReference>("import", "./js/firebaseMessaging.js");
        await _messagingModule.InvokeVoidAsync("initializeMessaging", _config);
    }

    public async Task<string?> RequestPermissionAndGetTokenAsync(string vapidKey)
    {
        await EnsureInitializedAsync();

        var token = await _messagingModule!.InvokeAsync<string?>(
            "requestPermissionAndGetToken", vapidKey, _config);

        if (token is not null)
        {
            _selfRef ??= DotNetObjectReference.Create(this);
            await _messagingModule.InvokeVoidAsync("registerOnMessage", _selfRef);
        }

        return token;
    }

    [JSInvokable]
    public void HandleIncomingMessage(string? title, string? body)
    {
        OnNotificationReceived?.Invoke(title, body);
    }

    public async ValueTask DisposeAsync()
    {
        if (_messagingModule is not null)
            await _messagingModule.DisposeAsync();

        _selfRef?.Dispose();
    }
}