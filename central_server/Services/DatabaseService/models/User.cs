using System.ComponentModel.DataAnnotations;

namespace central_server.Services.DatabaseService.models;

public class User(string email, string username, string passwordHash)
{
    public User() :
        this(string.Empty, string.Empty, string.Empty)
    {
    }

    [Key] public int Id { get; set; }

    public string Username { get; set; }

    [EmailAddress(ErrorMessage = "Invalid Email Address")]
    public string Email { get; set; }

    public string PasswordHash { get; set; }

    public bool IsAdmin { get; set; }
}