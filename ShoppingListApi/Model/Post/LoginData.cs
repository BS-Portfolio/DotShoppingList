namespace ShoppingListApi.Model.Post;

public class LoginData
{
    public string EmailAddress { get; }
    public string Password64 { get; }

    public LoginData(string emailAddress, string password64)
    {
        EmailAddress = emailAddress;
        Password64 = password64;    
    }
}