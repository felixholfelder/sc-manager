using Microsoft.JSInterop;
using sc_manager.Models;

namespace sc_manager.Services.Firestore;

public class DeviceTokenService(IJSRuntime js, FirebaseConfig config) : BaseFirestoreService(js, config)
{
    public async Task<List<DeviceToken>> GetTokensByUserId(string userId)
    {
        await EnsureInitializedAsync();
        return await _module!.InvokeAsync<List<DeviceToken>>("getTokensByUserId", userId);
    }
}
