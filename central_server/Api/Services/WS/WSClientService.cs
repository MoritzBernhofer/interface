using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace central_server.Services.WS;

public class WsClientService(ILogger<WsClientService> logger)
{
    private readonly ConcurrentDictionary<string, WebSocket> _sockets = new();
    public IEnumerable<string> ConnectedIds => _sockets.Keys;
    
    public string AddOrUpdate(WebSocket socket, string id)
    {
        logger.LogInformation("Connected socked with id: {ID}", id);
        _sockets[id] = socket;
        return id;
    }
    
    public async Task RemoveAsync(string id, bool closeIfOpen = true)
    {
        if (!_sockets.TryRemove(id, out var socket)) return;

        if (closeIfOpen && socket.State == WebSocketState.Open)
        {
            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure,
                                     "Server closing",
                                     CancellationToken.None);
        }
        logger.LogInformation("Disconnected socked with id: {ID}", id);
    }

    public async Task<bool> SendToAsync(string id, string message, CancellationToken ct = default)
    {
        if (!_sockets.TryGetValue(id, out var socket) || socket.State != WebSocketState.Open)
            return false;

        var bytes = Encoding.UTF8.GetBytes(message);
        await socket.SendAsync(bytes, WebSocketMessageType.Text, endOfMessage: true, ct);
        return true;
    }

    public async Task<int> BroadcastAsync(string message, CancellationToken ct = default)
    {
        var bytes = Encoding.UTF8.GetBytes(message);

        var tasks = _sockets.Values
                            .Where(s => s.State == WebSocketState.Open)
                            .Select(s => s.SendAsync(bytes, WebSocketMessageType.Text, true, ct)).ToList();

        await Task.WhenAll(tasks);
        return tasks.Count;
    }
}
