using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace sc_manager_server.Services;

public class FirebaseConfig
{
    public string ApiKey { get; set; } = "";
    public string AuthDomain { get; set; } = "";
    public string ProjectId { get; set; } = "";
    public string StorageBucket { get; set; } = "";
    public string MessagingSenderId { get; set; } = "";
    public string AppId { get; set; } = "";
}

public class AuthResult
{
    public bool Success { get; set; }
    public string? Email { get; set; }
    public string? Uid { get; set; }
    public string? Error { get; set; }
}

public class FirebaseAuthService : AuthenticationStateProvider, IAsyncDisposable
{
    private readonly IJSRuntime _js;
    private readonly FirebaseConfig _config;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private IJSObjectReference? _module;
    private DotNetObjectReference<FirebaseAuthService>? _dotnetRef;

    private ClaimsPrincipal _currentUser = new(new ClaimsIdentity());
    private bool _initialized;

    private readonly TaskCompletionSource _firstAuthStateReady = 
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    public FirebaseAuthService(IJSRuntime js, FirebaseConfig config, IHttpContextAccessor httpContextAccessor)
    {
        _js = js;
        _config = config;
        _httpContextAccessor = httpContextAccessor;
    }

    private async Task EnsureInitializedAsync()
    {
        if (_initialized) return;
        _initialized = true;

        _module = await _js.InvokeAsync<IJSObjectReference>("import", "./js/firebaseAuth.js");
        _dotnetRef = DotNetObjectReference.Create(this);

        var configObj = new
        {
            apiKey = _config.ApiKey,
            authDomain = _config.AuthDomain,
            projectId = _config.ProjectId,
            storageBucket = _config.StorageBucket,
            messagingSenderId = _config.MessagingSenderId,
            appId = _config.AppId
        };

        await _module.InvokeVoidAsync("initializeFirebase", configObj, _dotnetRef);
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        await EnsureInitializedAsync();

        var timeout = Task.Delay(TimeSpan.FromSeconds(8));
        await Task.WhenAny(_firstAuthStateReady.Task, timeout);

        return new AuthenticationState(_currentUser);
    }

    [JSInvokable]
    public Task OnAuthStateChanged(string? email, string? uid)
    {
        _currentUser = CreatePrincipal(email, uid);

        _firstAuthStateReady.TrySetResult();

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));
        return Task.CompletedTask;
    }

    private static ClaimsPrincipal CreatePrincipal(string? email, string? uid)
    {
        if (string.IsNullOrEmpty(email))
            return new ClaimsPrincipal(new ClaimsIdentity());

        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, email),
            new Claim(ClaimTypes.Email, email),
            new Claim("uid", uid ?? "")
        }, authenticationType: "firebase");

        return new ClaimsPrincipal(identity);
    }

    public async Task<AuthResult> LoginAsync(string email, string password)
    {
        await EnsureInitializedAsync();
        var result = await _module!.InvokeAsync<AuthResult>("signIn", email, password);
        
        if (result.Success)
        {
            var principal = CreatePrincipal(result.Email, result.Uid);
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext is not null)
            {
                await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
            }
            _currentUser = principal;
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));
        }

        return result;
    }

    public async Task LogoutAsync()
    {
        await EnsureInitializedAsync();
        await _module!.InvokeVoidAsync("logOut");
    }

    public async ValueTask DisposeAsync()
    {
        _dotnetRef?.Dispose();
        if (_module is not null)
        {
            await _module.DisposeAsync();
        }
    }
}
