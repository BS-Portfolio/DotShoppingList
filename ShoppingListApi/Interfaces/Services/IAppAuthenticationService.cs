using ShoppingListApi.Model.DTOs.Post;
using ShoppingListApi.Model.ReturnTypes;

namespace ShoppingListApi.Interfaces.Services;

public interface IAppAuthenticationService
{
    Task<ApiKeyAuthenticationResult> AuthenticateApiKeyAsync(Guid userId, string apiKey,
        CancellationToken cancellationToken = default);

    Task<LoginResult> LogIn(LoginDataDto loginDataDto,
        CancellationToken ct = default);

    Task<LogoutResult> LogOut(Guid requestingUserId, string providedApiKey, CancellationToken ct = default);
}