namespace App.Database.Model.Iot;

public class IotDevice
{
    public int Id { get; set; }
    public string IPv4 { get; set; }
    public string Name { get; set; }
    public List<IotService> IotService { get; set; }
}