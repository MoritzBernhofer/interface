using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace App.Database.Model.UserRelated;

public class User
{
    public int Id { get; set; }
    [MaxLength(100)]
    public required string Username { get; set; }
    [MaxLength(100)]
    public required string Password { get; set; }

    //navigational Property
    public Home? Home { get; set; }
    public List<UserLog> UserLogs { get; set; } = [];
}