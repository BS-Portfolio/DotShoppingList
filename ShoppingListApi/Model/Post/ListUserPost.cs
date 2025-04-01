using System.ComponentModel.DataAnnotations;
using BCrypt.Net;
namespace ShoppingListApi.Model.Post;

public class ListUserPost
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string EmailAddress { get; set; }
    public string PasswordHash { get; set; }
    public DateTimeOffset CreationDate { get; private set; }

    public ListUserPost(string firstName, string lastName, string emailAddress, string password, DateTimeOffset creationDate)
    {
        FirstName = firstName;
        LastName = lastName;
        EmailAddress = emailAddress;
        PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(password, 13);
        CreationDate = DateTimeOffset.UtcNow;
    }
}