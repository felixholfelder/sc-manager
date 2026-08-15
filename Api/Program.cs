using sc_manager.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<FirebaseFcmSenderService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClient", policy =>
        policy.WithOrigins("http://localhost:5062", "https://localhost:5062")
            .AllowAnyMethod()
            .AllowAnyHeader());
});

var app = builder.Build();

app.UseCors("AllowClient");

app.MapPost("/api/notifications/send", async (FirebaseFcmSenderService sender, SendRequest req) =>
{
    await sender.SendToTokenAsync(req.Token, req.Title, req.Body);
    return Results.Ok();
});

app.Run();

record SendRequest(string Token, string Title, string Body);
