namespace central_server.Api.IotDeviceLog;

public record IotDeviceLogDto(long Id, long CreatedAt, string Content, long IotDeviceId);