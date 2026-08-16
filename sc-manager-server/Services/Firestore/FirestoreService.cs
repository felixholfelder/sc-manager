using Microsoft.JSInterop;
using sc_manager_server.Services.Firestore;

namespace sc_manager_server.Services;

public class FirestoreService(IJSRuntime js, FirebaseConfig config) : BaseFirestoreService(js, config)
{
    public async Task<string> AddAsync<T>(string collectionName, T data)
    {
        await EnsureInitializedAsync();
        return await _module!.InvokeAsync<string>("addItem", collectionName, data);
    }

    public async Task<T?> GetAsync<T>(string collectionName, string id)
    {
        await EnsureInitializedAsync();
        return await _module!.InvokeAsync<T?>("getItem", collectionName, id);
    }

    public async Task<List<T>> GetAllAsync<T>(string collectionName)
    {
        await EnsureInitializedAsync();
        return await _module!.InvokeAsync<List<T>>("getItems", collectionName);
    }
    
    public async Task SetAsync(string collectionName, string id, object data)
    {
        await EnsureInitializedAsync();
        await _module!.InvokeVoidAsync("addItem", collectionName, id, data);
    }

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
}
