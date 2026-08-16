using Microsoft.JSInterop;
using sc_manager_server.Enums;
using sc_manager_server.Models;

namespace sc_manager_server.Services.Firestore;

public class UserService(IJSRuntime js, FirebaseConfig config) : BaseFirestoreService(js, config)
{
    public async Task<List<User>> GetUsersByType(UserTypeEnum userType)
    {
        await EnsureInitializedAsync();
        return await _module!.InvokeAsync<List<User>>("getUsersByType", userType);
    }
}
