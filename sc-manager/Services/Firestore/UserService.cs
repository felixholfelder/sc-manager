using Microsoft.JSInterop;
using sc_manager.Enums;
using sc_manager.Models;

namespace sc_manager.Services.Firestore;

public class UserService(IJSRuntime js, FirebaseConfig config) : BaseFirestoreService(js, config)
{
    public async Task<List<User>> GetUsersByType(UserTypeEnum userType)
    {
        await EnsureInitializedAsync();
        return await _module!.InvokeAsync<List<User>>("getUsersByType", userType);
    }
}
