using System.ComponentModel.DataAnnotations;
using System.Text;
using BCrypt.Net;
using Newtonsoft.Json;

namespace ShoppingListApi.Model.Post;

public class ListUserPostExtended
{
    [Required] public string FirstName { get; set; }
    [Required] public string LastName { get; set; }
    [Required] public string EmailAddress { get; set; }
    public string PasswordHash { get; private set; }
    public DateTimeOffset CreationDateTime { get; private set; }
    public string ApiKey { get; set; }
    public DateTimeOffset ApiKeyExpirationDateTime { get; set; }

    public ListUserPostExtended(string firstName, string lastName, string emailAddress, string password)
    {
        FirstName = firstName;
        LastName = lastName;
        EmailAddress = emailAddress;
        PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(password, 13);
        CreationDateTime = DateTimeOffset.UtcNow;
        ApiKey = HM.GenerateApiKey();
        ApiKeyExpirationDateTime = CreationDateTime + TimeSpan.FromHours(6);
    }

    public ListUserPostExtended(ListUserPost listUserPost)
    {
        FirstName = listUserPost.FirstName;
        LastName = listUserPost.LastName;

        byte[] decodedEmail64StringBytes = Convert.FromBase64String(listUserPost.EmailAddress64);
        string decodedEmailAddress = Encoding.UTF8.GetString(decodedEmail64StringBytes);
        EmailAddress = decodedEmailAddress;

        byte[] decodedPassword64StringBytes = Convert.FromBase64String(listUserPost.Password64);
        string decodedPassword = Encoding.UTF8.GetString(decodedPassword64StringBytes);
        PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(decodedPassword, 13);

        CreationDateTime = DateTimeOffset.UtcNow;
        ApiKey = HM.GenerateApiKey();
        ApiKeyExpirationDateTime = CreationDateTime.AddHours(6);
    }
}