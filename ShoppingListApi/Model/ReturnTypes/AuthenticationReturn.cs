namespace ShoppingListApi.Model.ReturnTypes;

public class AuthenticationReturn
{
    public bool AccountExists { get; }
    public bool IsAuthenticated { get; }
    public bool ApiKeyWasEqual { get; }
    public bool ApiKeyIsValid { get; }
    public bool? ProgramFailure { get; }

    public AuthenticationReturn(bool accountExists, bool isAuthenticated, bool apiKeyWasEqual, bool apiKeyIsValid,
        bool? programFailure = null)
    {
        AccountExists = accountExists;
        IsAuthenticated = isAuthenticated;
        ApiKeyWasEqual = apiKeyWasEqual;
        ApiKeyIsValid = apiKeyIsValid;
        ProgramFailure = programFailure;
    }
}