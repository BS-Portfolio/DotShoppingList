using ShoppingListApi.Model.ReturnTypes;

namespace ShoppingListApi.Interfaces.Services;

public interface IAppAuthenticationService
{
    Task<AppAuthenticationResult> AuthenticateAsync(Guid userId, string apiKey,
        CancellationToken cancellationToken = default);
}