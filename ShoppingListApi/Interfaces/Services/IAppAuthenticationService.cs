using ShoppingListApi.Model.DTOs.Post;
using ShoppingListApi.Model.ReturnTypes;

namespace ShoppingListApi.Interfaces.Services;

public interface IAppAuthenticationService
{
    /// <summary>
    /// Authenticates an API key for a user, checking validity and expiration.
    /// Returns an ApiKeyAuthenticationResult with detailed state flags.
    /// </summary>
    Task<ApiKeyAuthenticationResult> AuthenticateApiKeyAsync(Guid userId, string apiKey,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Attempts to log in a user with the provided credentials, creates a new API key if successful.
    /// Returns a LoginResult with state flags and user data.
    /// </summary>
    Task<LoginResult> LogIn(LoginDataDto loginDataDto,
        CancellationToken ct = default);

    /// <summary>
    /// Logs out a user by invalidating the provided API key.
    /// Returns a LogoutResult with state flags.
    /// </summary>
    Task<LogoutResult> LogOut(Guid requestingUserId, string providedApiKey, CancellationToken ct = default);
}