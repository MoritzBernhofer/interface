using System.Net.WebSockets;
using System.Text;
using central_server.Services.WS;

namespace central_server.WS;

public static class WSEndpoints
{
    public static void MapWebsocket(this WebApplication app)
    {
        app.UseWebSockets();
        app.Map("/ws", HandleRegisterWSClient);
    }

    private static async Task HandleRegisterWSClient(
        HttpContext context,
        WSClientService svc,
        ILogger<WSClientService> logger,
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
            logger.LogInformation("New WS client with id: {ID}", id);
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
                var res = await socket.ReceiveAsync(buf, ct);
                if (res.MessageType == WebSocketMessageType.Close) break;
            }
        }
        finally
        {
            await svc.RemoveAsync(id!);
            if (socket.State == WebSocketState.Open)
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure,
                    "Server shutdown", CancellationToken.None);
        }
    }
}