namespace central_server.Database.models;

public class IotDeviceLog
{
    public long Id { get; set; }
    public required long CreatedAt { get; set; }
    public required string Content { get; set; }
    public required long IotDeviceId { get; set; }
}