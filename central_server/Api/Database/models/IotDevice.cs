namespace central_server.Database.models;

public class IotDevice
{
    public long Id { get; set; }
    public required string Ipv4 { get; set; }
    public required long IotServiceId { get; set; }
    public required string Name { get; set; }
}