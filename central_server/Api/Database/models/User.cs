namespace central_server.Database.models;

public class User
{
    public long Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public List<ClientServer> ClientServers { get; set; } = [];
}