using System.ComponentModel.DataAnnotations;
using BCrypt.Net;
namespace ShoppingListApi.Model.Post;

public class ListUserPost
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string EmailAddress { get; private set; }
    public string PasswordHash { get; private set; }
    public DateTimeOffset CreationDateTime { get; private set; }
    public string ApiKey { get; private set; }
    public DateTimeOffset ApiKeyExpirationDateTime{ get; private set; }

    public ListUserPost(string firstName, string lastName, string emailAddress, string password, DateTimeOffset creationDate)
    {
        FirstName = firstName;
        LastName = lastName;
        EmailAddress = emailAddress;
        PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(password, 13);
        CreationDateTime = DateTimeOffset.UtcNow;
        ApiKey = HM.GenerateApiKey();
        ApiKeyExpirationDateTime = creationDate + TimeSpan.FromHours(6);
    }
}