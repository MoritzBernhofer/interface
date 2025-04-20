using Grpc.Core;
using GrpcReverse;

namespace central_server.Services.Grpc;

public class WorkerConnection
{
    public IServerStreamWriter<Acknowledgement> Stream { get; set; } = default!;
    public TaskCompletionSource<Acknowledgement>? Response { get; set; }
}
