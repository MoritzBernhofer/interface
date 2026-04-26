using System.ComponentModel.DataAnnotations;

namespace App.Database.Model.UserRelated;

public class Room
{
    public int Id { get; set; }
    [MaxLength(100)]
    public required string Name { get; set; }

    public int HomeId { get; set; }

    //navigational Property
    public Home? Home { get; set; }
}