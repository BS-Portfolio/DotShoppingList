using System.ComponentModel;

namespace ShoppingListApi.Enums;

public enum AuthorizationErrorEnum
{
    [Description("API Key is missing!")]
    ApiKeyIsMissing = 0,
    [Description("Wrong API Key!")]
    WrongApiKey = 1,
    [Description("User Account does not exist!")]
    UserAccountNotFound = 2,
    [Description("User API Key is not correct!")]
    InvalidUserApiKey = 3,
    [Description("User API Key is expired! Login in again!")]
    ExpiredApiKey = 4,
    [Description("Service Not Available!")]
    ServiceNotAvailable = 5,
    [Description("User Credentials Missing!")]
    UserCredentialsMissing = 6,
    [Description("User Credentials are provided in a wrong format!")]
    WrongFormat = 7,
    [Description("You are not part of this list!")]
    ListAccessNotGranted = 8,
    [Description("Your are not allowed to execute the requested action!")]
    ActionNotAllowed = 9,
    [Description("Login failure!")]
    LoginFailure = 10
}