using Grpc.Core;
using Grpc.Net.Client;
using GrpcReverse;
using static GrpcReverse.Dispatcher;

var channel = GrpcChannel.ForAddress("http://localhost:5001", new GrpcChannelOptions
{
    HttpHandler = new HttpClientHandler()
});

var client = new DispatcherClient(channel);

var call = client.Register();

var responseReader = Task.Run(async () =>
{
    await foreach (var message in call.ResponseStream.ReadAllAsync())
    {
        Console.WriteLine($"Received message: {message.Content}");

        await call.RequestStream.WriteAsync(new Acknowledgement
        {
            Id = message.Id,
            Status = "Received"
        });
    }
});

Console.WriteLine("Listening for messages...");
await responseReader;