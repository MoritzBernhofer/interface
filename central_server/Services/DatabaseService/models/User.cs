using System.ComponentModel.DataAnnotations;

namespace central_server.Services.DatabaseService.models;

public class User(string username, string passwordHash)
{
    [Key] public int Id { get; set; }

    [MaxLength(255)] public required string Username { get; set; } = username;

    [MaxLength(25556)] public required string PasswordHash { get; set; } = passwordHash;
}