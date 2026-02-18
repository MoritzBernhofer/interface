using System.ComponentModel.DataAnnotations.Schema;

namespace App.Database.Model.UserRelated;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }

    //navigational Property
    public Home Home { get; set; }
    public List<UserLog> UserLogs { get; set; }
}