using System.Text;

namespace ShoppingListApi.Model.DTOs.Post;

public class LoginDataDto
{
    public string EmailAddress { get; }
    public string Password { get; }

    public LoginDataDto(string emailAddress, string password)
    {
        byte[] decodedEmail64StringBytes = Convert.FromBase64String(emailAddress);
        string decodedEmailAddress = Encoding.UTF8.GetString(decodedEmail64StringBytes);
        EmailAddress = decodedEmailAddress;
        
        byte[] decodedPassword64StringBytes = Convert.FromBase64String(password);
        string decodedPassword = Encoding.UTF8.GetString(decodedPassword64StringBytes);
        Password = decodedPassword;
    }
}