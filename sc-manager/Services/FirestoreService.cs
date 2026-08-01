using Microsoft.JSInterop;

namespace sc_manager.Services;

/// <summary>
/// Minimaler, generischer Firestore-Service.
/// Verwendet dieselbe FirebaseConfig wie FirebaseAuthService (in Program.cs registriert).
/// </summary>
public class FirestoreService : IAsyncDisposable
{
    private readonly IJSRuntime _js;
    private readonly FirebaseConfig _config;
    private IJSObjectReference? _module;
    private bool _initialized;

    public FirestoreService(IJSRuntime js, FirebaseConfig config)
    {
        _js = js;
        _config = config;
    }

    private async Task EnsureInitializedAsync()
    {
        if (_initialized) return;
        _initialized = true;

        _module = await _js.InvokeAsync<IJSObjectReference>("import", "./js/firestore.js");

        var configObj = new
        {
            apiKey = _config.ApiKey,
            authDomain = _config.AuthDomain,
            projectId = _config.ProjectId,
            storageBucket = _config.StorageBucket,
            messagingSenderId = _config.MessagingSenderId,
            appId = _config.AppId
        };

        await _module.InvokeVoidAsync("initializeFirestore", configObj);
    }

    /// <summary>Fügt ein Dokument hinzu und gibt dessen generierte ID zurück.</summary>
    public async Task<string> AddAsync<T>(string collectionName, T data)
    {
        await EnsureInitializedAsync();
        return await _module!.InvokeAsync<string>("addItem", collectionName, data);
    }

    /// <summary>Liest ein einzelnes Dokument. Gibt default(T) zurück, falls nicht vorhanden.</summary>
    public async Task<T?> GetAsync<T>(string collectionName, string id)
    {
        await EnsureInitializedAsync();
        return await _module!.InvokeAsync<T?>("getItem", collectionName, id);
    }

    /// <summary>Liest alle Dokumente einer Collection, neueste zuerst.</summary>
    public async Task<List<T>> GetAllAsync<T>(string collectionName)
    {
        await EnsureInitializedAsync();
        return await _module!.InvokeAsync<List<T>>("getItems", collectionName);
    }

    /// <summary>Aktualisiert einzelne Felder eines Dokuments (Merge, kein Overwrite).</summary>
    public async Task UpdateAsync(string collectionName, string id, object partialData)
    {
        await EnsureInitializedAsync();
        await _module!.InvokeVoidAsync("updateItem", collectionName, id, partialData);
    }

    public async Task DeleteAsync(string collectionName, string id)
    {
        await EnsureInitializedAsync();
        await _module!.InvokeVoidAsync("deleteItem", collectionName, id);
    }
    
    /// <summary>Liest alle Dokumente, bei denen "field" genau "value" entspricht.</summary>
    public async Task<List<T>> GetListWhereAsync<T>(string collectionName, string field, object value)
    {
        await EnsureInitializedAsync();
        return await _module!.InvokeAsync<List<T>>("getItemsByField", collectionName, field, value);
    }
    
    public async Task<T> GetWhereAsync<T>(string collectionName, string field, object value)
    {
        await EnsureInitializedAsync();
        return await _module!.InvokeAsync<T>("getItemByField", collectionName, field, value);
    }

    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
        {
            await _module.DisposeAsync();
        }
    }
}
