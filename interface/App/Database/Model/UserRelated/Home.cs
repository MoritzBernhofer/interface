namespace App.Database.Model.UserRelated;

public class Home
{
    public int Id { get; set; }
    public string Name  {get; set;}

    public int UserId { get; set; }
    //navigational Property

    public User User { get; set; }
}