using System.ComponentModel.DataAnnotations;

namespace ShoppingListApi.Model.Post;

public class ListUserPost
{
    [Required] public string FirstName { get; set; }
    [Required] public string LastName { get; set; }
    [Required] public string EmailAddress { get; set; }
    [Required] public string Password64 { get; set; }

    public ListUserPost(string firstName, string lastName, string emailAddress, string password64)
    {
        FirstName = firstName;
        LastName = lastName;
        EmailAddress = emailAddress;
        Password64 = password64;
    }
}