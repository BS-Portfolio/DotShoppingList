using System.ComponentModel.DataAnnotations;

namespace ShoppingListApi.Model.Post;

public class ListUserPost
{
    [Required] public string FirstName { get; set; }
    [Required] public string LastName { get; set; }
    [Required] public string EmailAddress64 { get; set; }
    [Required] public string Password64 { get; set; }

    public ListUserPost(string firstName, string lastName, string emailAddress64, string password64)
    {
        FirstName = firstName;
        LastName = lastName;
        EmailAddress64 = emailAddress64;
        Password64 = password64;
    }
}