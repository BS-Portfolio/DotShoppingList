using System.ComponentModel.DataAnnotations;

namespace ShoppingList.Server.Model.Post;

public class User
{
    public int UserID { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string EmailAddress { get; set; }
    public string PasswordHash { get; set; }
    public DateTime CreationDate { get; set; }
}