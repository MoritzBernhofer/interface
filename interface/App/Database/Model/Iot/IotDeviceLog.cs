namespace App.Database.Model.Iot;

public class IotDeviceLog
{
    public int Id { get; set; }
    public float CreatedAt { get; set; }
    public string Content  { get; set; }
    public int IotDeviceId { get; set; }

    //navigational Property
    public IotDevice IotDevice { get; set; }
}