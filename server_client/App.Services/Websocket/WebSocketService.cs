using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Api.Services.Handler;
using Microsoft.Extensions.Logging;

namespace Api.Services.Websocket;

public class WebSocketService(RequestServiceHandler serviceHandler, ILogger<WebSocketService> logger)
{
    public async Task StartAsync(string uri, bool expectWelcome, string idFile, CancellationToken appCt)
    {
        var backoff = TimeSpan.FromSeconds(1);
        var buf = new byte[4 * 1024];

        while (!appCt.IsCancellationRequested)
        {
            using var ws = new ClientWebSocket();

            try
            {
                logger.LogInformation("connecting to {Uri} …", uri);
                await ws.ConnectAsync(new Uri(uri), appCt);
                logger.LogInformation("connected ✔");

                backoff = TimeSpan.FromSeconds(1);

                while (ws.State == WebSocketState.Open && !appCt.IsCancellationRequested)
                {
                    var res = await ws.ReceiveAsync(buf, appCt);
                    if (res.MessageType == WebSocketMessageType.Close) break;

                    var msg = Encoding.UTF8.GetString(buf, 0, res.Count);

                    if (expectWelcome && msg.StartsWith("CLIENT_ID:"))
                    {
                        var id = msg["CLIENT_ID:".Length..];
                        await File.WriteAllTextAsync(idFile, id, appCt);
                        logger.LogInformation("assigned id = {ID} (saved to {IDFile})", id, idFile);
                        expectWelcome = false;
                        continue;
                    }

                    try
                    {
                        var typedMsg = JsonSerializer.Deserialize<RequestDto>(msg);
                        await serviceHandler.Handle(typedMsg);
                    }
                    catch (JsonException ex)
                    {
                        logger.LogWarning(ex, "Failed to deserialize message: {Message}", msg);
                    }
                }
            }
            catch (OperationCanceledException) when (appCt.IsCancellationRequested) { break; }
            catch (Exception ex)
            {
                logger.LogWarning(ex.Message);
            }

            if (appCt.IsCancellationRequested) break;
            logger.LogInformation($"retrying in {backoff.TotalSeconds}s …");
            try { await Task.Delay(backoff, appCt); } catch (OperationCanceledException) { break; }
            backoff = TimeSpan.FromSeconds(Math.Min(backoff.TotalSeconds * 2, 30));
        }
    }
}