using ShoppingListApi.Model.ReturnTypes;

namespace ShoppingListApi.Interfaces.Services;

public interface IAppAuthenticationService
{
    Task<AppAuthenticationResult> AuthenticateAsync();
}