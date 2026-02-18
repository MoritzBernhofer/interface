namespace App.Database.Model.UserRelated;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }

    public int HomeId { get; set; }


    //navigational Property
    public Home Home { get; set; }
}