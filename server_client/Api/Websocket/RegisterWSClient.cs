using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace WebApplication1.Websocket;

public static class RegisterWSClient
{
    public static void MapWSEndpoints(this WebApplication app)
    {

        app.Map("/ws", async context =>                      // 08: Handle requests that reach the “/ws” path
        {
            if (!context.WebSockets.IsWebSocketRequest)      // 09: Reject plain HTTP calls
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest; // 10: 400 = Bad request
                return;                                      // 11: End request pipeline
            }

            using var socket = await context.WebSockets.AcceptWebSocketAsync(); // 12: Perform the HTTP→WebSocket upgrade
            var id = Guid.NewGuid();                         // 13: Assign a fresh ID to this connection
            sockets[id] = socket;                            // 14: Remember the socket for later “push” messages

            var buffer = new byte[4 * 1024];                 // 15: 4 KiB receive buffer (text frames are small)

            try                                              // 16: Keep reading until the client closes
            {
                while (socket.State == WebSocketState.Open)  // 17: Stay in the loop while the TCP connection lives
                {
                    var result = await socket.ReceiveAsync(buffer, CancellationToken.None); // 18: Await incoming frames
                    if (result.MessageType == WebSocketMessageType.Close)                   // 19: Client requests close?
                        break;                                      // 20: Exit loop → finally{} cleans up below
                    /* optional: handle text/binary here if you need two-way chat */
                }
            }
            finally
            {
                sockets.TryRemove(id, out _);               // 22: Drop the socket so nobody sends to a dead client
            }
        }); // 23: End of “/ws” handler
    }
}