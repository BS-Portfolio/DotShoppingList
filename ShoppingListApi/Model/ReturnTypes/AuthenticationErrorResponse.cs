using System.ComponentModel;
using ShoppingListApi.Enums;
using System.Reflection;

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
/// 8: You are not part of this list!
/// 9: You are not allowed to execute the requested action!
/// 10: Login failure!
/// </summary>
public class AuthenticationErrorResponse
{
    public int AuthenticationErrorCode { get; }
    public string Message { get; }

    public AuthenticationErrorResponse(AuthorizationErrorEnum authEnum)
    {
        AuthenticationErrorCode = (int)authEnum;
        Message = GetEnumDescription(authEnum);
    }

    private static string GetEnumDescription(Enum value)
    {
        var fi = value.GetType().GetField(value.ToString());

        if (fi is null) return value.ToString();
        
        DescriptionAttribute[] attributes =
            (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

        return (attributes.Length > 0) ? attributes[0].Description : value.ToString();
    }
}