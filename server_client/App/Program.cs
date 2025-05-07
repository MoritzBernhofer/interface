using System.Net.WebSockets;
using System.Text;

const string WS_BASE = "ws://91.142.26.98:5000/ws";
const string ID_FILE = "client-id.txt";

string? clientId = File.Exists(ID_FILE) ? await File.ReadAllTextAsync(ID_FILE) : null;
string   wsUri   = clientId is null ? WS_BASE : $"{WS_BASE}?id={clientId}";

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e)=>{ e.Cancel=true; cts.Cancel(); };

await RunAsync(wsUri, clientId is null, cts.Token);

return;


static async Task RunAsync(string uri, bool expectWelcome, CancellationToken appCt)
{
    var backoff = TimeSpan.FromSeconds(1);
    var buf     = new byte[4 * 1024];

    while (!appCt.IsCancellationRequested)
    {
        using var ws = new ClientWebSocket();

        try
        {
            Console.WriteLine($"[info] connecting to {uri} …");
            await ws.ConnectAsync(new Uri(uri), appCt);
            Console.WriteLine("[info] connected ✔\n");

            backoff = TimeSpan.FromSeconds(1);               // reset back‑off

            // ── receive loop ─────────────────────────────
            while (ws.State == WebSocketState.Open && !appCt.IsCancellationRequested)
            {
                var res = await ws.ReceiveAsync(buf, appCt);
                if (res.MessageType == WebSocketMessageType.Close) break;

                var msg = Encoding.UTF8.GetString(buf, 0, res.Count);

                if (expectWelcome && msg.StartsWith("CLIENT_ID:"))
                {
                    var id = msg["CLIENT_ID:".Length..];
                    await File.WriteAllTextAsync(ID_FILE, id, appCt);
                    Console.WriteLine($"[info] assigned id = {id} (saved to {ID_FILE})");
                    expectWelcome = false;                 // welcome handled
                    continue;
                }

                Console.WriteLine(msg);
            }
        }
        catch (OperationCanceledException) when (appCt.IsCancellationRequested) { break; }
        catch (Exception ex) { Console.WriteLine($"[warn] {ex.Message}"); }

        // ── reconnect with exponential back‑off ───────────────────────────
        if (appCt.IsCancellationRequested) break;
        Console.WriteLine($"[info] retrying in {backoff.TotalSeconds}s …\n");
        try { await Task.Delay(backoff, appCt); } catch (OperationCanceledException) { break; }
        backoff = TimeSpan.FromSeconds(Math.Min(backoff.TotalSeconds * 2, 30));
    }
}
