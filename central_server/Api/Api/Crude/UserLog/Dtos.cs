namespace central_server.Api.UserLog;

public record UserLogDto(long Id, long CreatedAt, string Content, long UserId);