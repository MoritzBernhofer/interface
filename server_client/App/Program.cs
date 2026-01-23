using Api.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

//services
var services = new ServiceCollection();

services.AddSingleton<HttpClient>();
//Websocket services
services.AddSingleton<WebSocketService>();


services.AddLogging(configure => configure.AddConsole());

var serviceProvider = services.BuildServiceProvider();
var wsService = serviceProvider.GetRequiredService<WebSocketService>();

const string wsBase = "ws://91.142.26.98:5000/ws";
const string idFile = "client-id.txt";

var clientId = File.Exists(idFile) ? await File.ReadAllTextAsync(idFile) : null;
var wsUri = clientId is null ? wsBase : $"{wsBase}?id={clientId}";

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e)=>{ e.Cancel=true; cts.Cancel(); };

await wsService.StartAsync(wsUri, clientId is null, idFile, cts.Token);