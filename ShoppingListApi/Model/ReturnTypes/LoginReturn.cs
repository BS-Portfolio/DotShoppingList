namespace ShoppingListApi.Model.ReturnTypes;

public class LoginReturn
{
    public bool LoginSuccessful { get; }
    public string? ApiKey { get; }
    public DateTimeOffset? ApiKeyExpirationDateTime { get; }

    public LoginReturn(bool loginSuccessful, string? apiKey = null, DateTimeOffset? apiKeyExpirationDateTime = null)
    {
        LoginSuccessful = loginSuccessful;
        ApiKey = apiKey;
        ApiKeyExpirationDateTime = apiKeyExpirationDateTime;
    }
}