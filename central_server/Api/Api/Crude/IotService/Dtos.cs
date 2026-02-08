namespace central_server.Api.IotService;

public record IotServiceDto(long Id, string Name);

public record CreateIotServiceDto(string Name);
