using System.Net.WebSockets;
using System.Text;

const string wsBase = "ws://91.142.26.98:5000/ws";
const string idFile = "client-id.txt";

string? clientId = File.Exists(idFile) ? await File.ReadAllTextAsync(idFile) : null;
string   wsUri   = clientId is null ? wsBase : $"{wsBase}?id={clientId}";

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
                    Console.WriteLine($"[info] assigned id = {id} (saved to {idFile})");
                    expectWelcome = false;
                    continue;
                }

                Console.WriteLine(msg);
            }
        }
        catch (OperationCanceledException) when (appCt.IsCancellationRequested) { break; }
        catch (Exception ex) { Console.WriteLine($"[warn] {ex.Message}"); }

        if (appCt.IsCancellationRequested) break;
        Console.WriteLine($"[info] retrying in {backoff.TotalSeconds}s …\n");
        try { await Task.Delay(backoff, appCt); } catch (OperationCanceledException) { break; }
        backoff = TimeSpan.FromSeconds(Math.Min(backoff.TotalSeconds * 2, 30));
    }
}
