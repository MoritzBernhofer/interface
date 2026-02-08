namespace central_server.Api.User;

public record UserDto(long Id, string Name, string Email);

public record CreateUserDto(string Name, string Email, string Password);
