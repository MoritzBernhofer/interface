namespace central_server.Database.models;

public class UserLog
{
    public long Id { get; set; }
    public required long CreatedAt { get; set; }
    public required string Content {get; set;}
    public required long UserId { get; set; }
}