using Microsoft.JSInterop;

namespace sc_manager_server.Services.Firestore;

public abstract class BaseFirestoreService(IJSRuntime js, FirebaseConfig config) : IAsyncDisposable
{
    protected IJSObjectReference? _module;
    private bool _initialized;

    public async Task EnsureInitializedAsync()
    {
        if (_initialized) return;
        _initialized = true;

        _module = await js.InvokeAsync<IJSObjectReference>("import", "./js/firestore.js");

        var configObj = new
        {
            apiKey = config.ApiKey,
            authDomain = config.AuthDomain,
            projectId = config.ProjectId,
            storageBucket = config.StorageBucket,
            messagingSenderId = config.MessagingSenderId,
            appId = config.AppId
        };

        await _module.InvokeVoidAsync("initializeFirestore", configObj);
    }
    
    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
        {
            await _module.DisposeAsync();
        }
    }
}