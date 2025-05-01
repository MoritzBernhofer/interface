using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace central_server.Services.WS;

public class WSClientService
{
    private readonly ConcurrentDictionary<Guid, WebSocket> _sockets = new();


    public IEnumerable<Guid> ConnectedIds => _sockets.Keys;


    public Guid Add(WebSocket socket)
    {
        var id = Guid.NewGuid();
        _sockets[id] = socket;
        return id;
    }

    public async Task<bool> RemoveAsync(Guid id, bool closeIfOpen = true)
    {
        if (!_sockets.TryRemove(id, out var socket))
            return false;

        if (closeIfOpen && socket.State == WebSocketState.Open)
        {
            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server closing", CancellationToken.None);
        }
        return true;
    }

    public async Task<bool> SendToAsync(Guid id, string message, CancellationToken ct = default)
    {
        if (!_sockets.TryGetValue(id, out var socket) || socket.State != WebSocketState.Open)
            return false;

        var bytes = Encoding.UTF8.GetBytes(message);
        await socket.SendAsync(bytes, WebSocketMessageType.Text, true, ct);
        return true;
    }

    public async Task<int> BroadcastAsync(string message, CancellationToken ct = default)
    {
        var bytes = Encoding.UTF8.GetBytes(message);

        var tasks = _sockets.Values
                            .Where(s => s.State == WebSocketState.Open)
                            .Select(s => s.SendAsync(bytes, WebSocketMessageType.Text, true, ct));

        await Task.WhenAll(tasks);
        return tasks.Count();
    }
}
