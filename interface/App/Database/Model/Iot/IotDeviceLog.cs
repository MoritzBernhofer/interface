using System.ComponentModel.DataAnnotations;

namespace App.Database.Model.Iot;

public class IotDeviceLog
{
    public int Id { get; set; }
    public float CreatedAt { get; set; }
    [MaxLength(4000)] public required string Content { get; set; }
    public int IotDeviceId { get; set; }

    //navigational Property
    public IotDevice? IotDevice { get; set; }
}