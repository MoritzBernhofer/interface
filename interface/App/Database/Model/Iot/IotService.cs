namespace App.Database.Model.Iot;

public class IotService
{
    public int Id { get; set; }
    public string Name { get; set; }

    public string Url { get; set; }
    public string Body { get; set; }
    public int SleepTime { get; set; }

    public IotServiceType Type { get; set; }
    public int IotDeviceId { get; set; }

    public IotDevice IotDevice { get; set; }
}

public enum IotServiceType
{
    Http
}