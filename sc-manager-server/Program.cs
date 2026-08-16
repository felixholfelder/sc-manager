using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using sc_manager_server.Components;
using sc_manager_server.Services;
using sc_manager_server.Services.Firestore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton(new FirebaseConfig
{
    ApiKey = builder.Configuration["Firebase:ApiKey"],
    AuthDomain = builder.Configuration["Firebase:AuthDomain"],
    ProjectId = builder.Configuration["Firebase:ProjectId"],
    StorageBucket = builder.Configuration["Firebase:StorageBucket"],
    MessagingSenderId = builder.Configuration["Firebase:MessagingSenderId"],
    AppId = builder.Configuration["Firebase:AppId"]
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.AccessDeniedPath = "/Login";
    });

builder.Services.AddAuthorizationCore();

builder.Services.AddScoped<FirebaseAuthService>();
builder.Services.AddScoped<FirestoreService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<DeviceTokenService>();
builder.Services.AddScoped<FirebaseFcmSenderService>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<FirebaseAuthService>());
builder.Services.AddHttpContextAccessor();

builder.Services.AddCascadingAuthenticationState();


var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found",
    createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets().AllowAnonymous();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
