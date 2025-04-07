using System.Text;

namespace ShoppingListApi.Model.Post;

public class LoginData
{
    public string EmailAddress { get; }
    public string Password { get; }

    public LoginData(string emailAddress, string password)
    {
        byte[] decodedEmail64StringBytes = Convert.FromBase64String(emailAddress);
        string decodedEmailAddress = Encoding.UTF8.GetString(decodedEmail64StringBytes);
        EmailAddress = decodedEmailAddress;
        
        byte[] decodedPassword64StringBytes = Convert.FromBase64String(password);
        string decodedPassword = Encoding.UTF8.GetString(decodedPassword64StringBytes);
        Password = decodedPassword;
    }
}