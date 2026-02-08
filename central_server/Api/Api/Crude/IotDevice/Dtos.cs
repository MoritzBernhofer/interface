namespace central_server.Api.IotDevice;

public record IotDeviceDto(long Id, string Ipv4, long IotServiceId, string Name);

public record CreateIotDeviceDto(string Ipv4, long IotServiceId, string Name);
