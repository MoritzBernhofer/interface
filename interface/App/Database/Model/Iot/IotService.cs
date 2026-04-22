using System.ComponentModel.DataAnnotations;

namespace App.Database.Model.Iot;

public class IotService
{
    public int Id { get; set; }
    public required string Name { get; set; }

    [MaxLength(500)]
    public required string Url { get; set; }
    [MaxLength(20000)]

    public required string Body { get; set; }
    public int SleepTime { get; set; }

    public IotServiceType Type { get; set; }
    public int IotDeviceId { get; set; }

    //Navigational Property
    public IotDevice? IotDevice { get; set; }
}

public enum IotServiceType
{
    Http
}