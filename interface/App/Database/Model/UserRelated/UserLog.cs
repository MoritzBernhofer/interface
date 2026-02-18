namespace App.Database.Model.UserRelated;

public class UserLog
{
    public int Id { get; set; }
    public float CreatedAt { get; set; }
    public string Content { get; set; }
    public int UserId { get; set; }

    //navigational Property
    public User User { get; set; }
}