using Api.Services;
using Api.Services.Iot;
using App.Api;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddSingleton<WebSocketService>();
builder.Services.AddSingleton<CLogger>();
builder.Services.AddSingleton<IotWorkflowManager>();
builder.Services.AddSingleton<MessageHandler>();

builder.Services.AddHttpClient();

builder.Services.AddHostedService(sp => sp.GetRequiredService<IotWorkflowManager>());




var app = builder.Build();

// endpoints

app.MapInfoEndpoints();

// Start WebSocket service in background
var wsService = app.Services.GetRequiredService<WebSocketService>();

const string wsBase = "ws://91.142.26.98:5000/ws";
const string idFile = "client-id.txt";

string? clientId;

if (File.Exists(idFile))
    clientId = await File.ReadAllTextAsync(idFile);
else
    clientId = null;

var wsUri = clientId switch
{
    null => wsBase,
    _ => $"{wsBase}?id={clientId}"
};

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

_ = wsService.StartAsync(wsUri, clientId is null, idFile, cts.Token);

app.Run();