namespace central_server.Api.ClientServer;

public record ClientServerDto(long Id, string Name);

public record CreateClientServerDto(string Name);
