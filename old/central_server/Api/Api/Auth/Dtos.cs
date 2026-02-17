namespace central_server.Api.Auth;

public record LoginDto(string Email, string Password);

public record LoginResponseDto(string Token, long UserId, string Email, string Name);
