using System.ComponentModel.DataAnnotations;

namespace central_server.Services.DatabaseService.models;

public class Server(
    string name)
{
    [Key] public int Id { get; set; }

    [MaxLength(255)] public required string Name { get; set; } = name;
}