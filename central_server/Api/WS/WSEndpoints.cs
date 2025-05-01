using System.Net.WebSockets;
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
        HttpContext        context,
        WSClientService    wsClientService,
        CancellationToken  ct = default)
    {
        if (!context.WebSockets.IsWebSocketRequest)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("Expected a WebSocket upgrade.", ct);
            return;
        }

        using var socket = await context.WebSockets.AcceptWebSocketAsync();
        var id = Guid.NewGuid();
        wsClientService.Add(socket);

        var buffer = new byte[4 * 1024];

        try
        {
            while (!ct.IsCancellationRequested && socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(buffer, ct);

                if (result.MessageType == WebSocketMessageType.Close)
                    break;
            }
        }
        finally
        {
            await wsClientService.RemoveAsync(id);
            if (socket.State != WebSocketState.Closed)
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server shutdown", ct);
        }
    }
}