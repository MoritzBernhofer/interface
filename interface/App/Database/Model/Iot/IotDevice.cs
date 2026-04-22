namespace App.Database.Model.Iot;

public class IotDevice
{
    public int Id { get; set; }
    public required string IPv4 { get; set; }
    public required string Name { get; set; }

    //navigational Property
    public List<IotService> IotService { get; set; } = [];
    public List<IotDeviceLog> IotDeviceLogs { get; set; } = [];
}