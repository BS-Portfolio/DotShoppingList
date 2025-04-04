namespace ShoppingListApi.Model.ReturnTypes;

/// <summary>
/// Codes:
/// 0: API Key is missing!
/// 1: Wrong API Key!
/// 2: User Account does not exist!
/// 3: User API Key is not correct!
/// 4: User API Key is expired! Login in again!
/// 5: Service Not Available!
/// 6: User Credentials Missing!
/// 7: User Credentials are provided in a wrong format!
/// </summary>
public class AuthenticationErrorReturn
{
    public int AuthenticationErrorCode { get; }
    public string Message { get; }

    public AuthenticationErrorReturn(int authenticationErrorCode, string message)
    {
        AuthenticationErrorCode = authenticationErrorCode;
        Message = message;
    }
}