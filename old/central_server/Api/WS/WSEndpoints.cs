using System.Net.WebSockets;
using System.Text;
using central_server.Logging;
using central_server.Services.WS;

namespace central_server.WS;

public static class WsEndpoints
{
    public static void MapWebsocket(this WebApplication app)
    {
        app.UseWebSockets();
        app.Map("/ws", HandleRegisterWsClient);
    }

    private static async Task HandleRegisterWsClient(
        HttpContext context,
        WsClientService svc,
        CLogger logger,
        WsReceiver handler,
        CancellationToken ct = default)
    {
        if (!context.WebSockets.IsWebSocketRequest)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("Expected WebSocket upgrade.", ct);
            return;
        }

        using var socket = await context.WebSockets.AcceptWebSocketAsync();

        var id = context.Request.Query["id"].FirstOrDefault();
        var isNew = string.IsNullOrWhiteSpace(id);
        if (isNew)
        {
            id = Guid.NewGuid().ToString();
            logger.LogInformation($"New WS client with id: {id}");
        }

        svc.AddOrUpdate(socket, id!);

        if (isNew && socket.State == WebSocketState.Open)
        {
            var bytes = Encoding.UTF8.GetBytes($"CLIENT_ID:{id}");
            await socket.SendAsync(bytes, WebSocketMessageType.Text, true, ct);
        }

        var buf = new byte[4 * 1024];
        try
        {
            while (!ct.IsCancellationRequested && socket.State == WebSocketState.Open)
            {
                try
                {
                    var res = await socket.ReceiveAsync(buf, ct);
                    if (res.MessageType == WebSocketMessageType.Close) break;
                    
                    var content = Encoding.UTF8.GetString(buf, 0, res.Count);
                    
                    var response = await handler.Handle(content);
                    
                }
                catch (WebSocketException wsex) when (wsex.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
                {
                    logger.LogWarning($"Client {id} closed connection without proper close handshake");
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError($"Unexpected error in WebSocket receive loop for client {id}");
                    break;
                }
            }
        }
        finally
        {
            await svc.RemoveAsync(id!);
            if (socket.State == WebSocketState.Open)
            {
                try
                {
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server shutdown", CancellationToken.None);
                }
                catch (Exception ex)
                {
                    logger.LogWarning($"Error while closing WebSocket for client {id}", ex);
                }
            }

            logger.LogInformation($"Disconnected socket with id: {id}");
        }
    }
}
