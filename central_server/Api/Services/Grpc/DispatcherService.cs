using System.Collections.Concurrent;
using Grpc.Core;
using GrpcReverse;

namespace central_server.Services.Grpc;

public class DispatcherService : Dispatcher.DispatcherBase
{
    private static readonly ConcurrentDictionary<string, IServerStreamWriter<Message>> Workers = new();

    public override async Task Register(
        IAsyncStreamReader<Acknowledgement> requestStream,
        IServerStreamWriter<Message> responseStream,
        ServerCallContext context)
    {
        var workerId = Guid.NewGuid().ToString();
        Console.WriteLine($"Worker connected: {workerId}");
        Workers[workerId] = responseStream;

        try
        {
            await foreach (var ack in requestStream.ReadAllAsync())
            {
                Console.WriteLine($"Ack from worker {workerId}: {ack.Id} - {ack.Status}");
            }
        }
        finally
        {
            Workers.TryRemove(workerId, out _);
            Console.WriteLine($"Worker disconnected: {workerId}");
        }
    }

    public static async Task BroadcastMessageAsync(string content)
    {
        var message = new Message
        {
            Id = Guid.NewGuid().ToString(),
            Content = content
        };

        foreach (var (workerId, stream) in Workers)
        {
            try
            {
                await stream.WriteAsync(message);
                Console.WriteLine($"Sent to {workerId}: {content}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send to {workerId}: {ex.Message}");
            }
        }
    }
}