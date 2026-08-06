using sc_manager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using sc_manager;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

// --- Firebase-Konfiguration -------------------------------------------------
// Werte aus: Firebase Console -> Projekteinstellungen -> Deine Apps -> SDK-Setup
string apiKey = builder.Configuration["Firebase:ApiKey"];
string authDomain = builder.Configuration["Firebase:AuthDomain"];
string projectId = builder.Configuration["Firebase:ProjectId"];
string storageBucket = builder.Configuration["Firebase:StorageBucket"];
string messagingSenderId = builder.Configuration["Firebase:MessagingSenderId"];
string appId = builder.Configuration["Firebase:AppId"];

// Oder strongly-typed über ein Modell:
builder.Services.AddSingleton(new FirebaseConfig
{
    ApiKey = apiKey,
    AuthDomain = authDomain,
    ProjectId = projectId,
    StorageBucket = storageBucket,
    MessagingSenderId = messagingSenderId,
    AppId = appId,
});

// --- Authentifizierung -------------------------------------------------------
builder.Services.AddAuthorizationCore(options =>
{
    // Standard: JEDE Seite verlangt einen angemeldeten Benutzer,
    // ausser Seiten mit [AllowAnonymous] (z. B. /login)
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddScoped<FirebaseAuthService>();
builder.Services.AddScoped<FirestoreService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<FirebaseAuthService>());

await builder.Build().RunAsync();