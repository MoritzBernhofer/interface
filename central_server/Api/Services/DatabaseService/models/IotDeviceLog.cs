using System.ComponentModel.DataAnnotations.Schema;

namespace central_server.Services.DatabaseService.models;

public class IotDeviceLog
{
    public long Id { get; set; }
    public required long CreatedAt { get; set; }
    public required string Content { get; set; }
    public required long IotDeviceId { get; set; }
}