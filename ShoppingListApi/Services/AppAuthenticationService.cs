using System.Text;
using Newtonsoft.Json;
using ShoppingListApi.Configs;
using ShoppingListApi.Enums;
using ShoppingListApi.Exceptions;
using ShoppingListApi.Interfaces.Services;
using ShoppingListApi.Model.DTOs.Get;
using ShoppingListApi.Model.DTOs.Post;
using ShoppingListApi.Model.Entity;
using ShoppingListApi.Model.ReturnTypes;

namespace ShoppingListApi.Services;

public class AppAuthenticationService(IUnitOfWork unitOfWork, ILogger<AppAuthenticationService> logger)
    : IAppAuthenticationService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<AppAuthenticationService> _logger = logger;

    /// <summary>
    /// Authenticates an API key for a user, checking validity and expiration.
    /// Returns an ApiKeyAuthenticationResult with detailed state flags.
    /// </summary>
    public async Task<ApiKeyAuthenticationResult> AuthenticateApiKeyAsync(Guid userId, string apiKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var targetUser = await _unitOfWork.ListUserRepository.GetWithoutDetailsByIdAsync(userId, cancellationToken);

            if (targetUser is null)
                return new(false, false, null, null, null);

            var targetApiKey =
                await _unitOfWork.ApiKeyRepository.GetByKeyForUserAsync(userId, apiKey, cancellationToken);

            if (targetApiKey is null)
                return new(true, false, false, null, null);

            if (targetApiKey.IsValid is false)
                return new(true, false, true, false, false);

            if (targetApiKey.ExpirationDateTime <= DateTimeOffset.UtcNow)
                return new(true, false, true, true, true);

            return new(true, true, true, true, false);
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(AppAuthenticationService), nameof(AuthenticateApiKeyAsync));
            throw numberedException;
        }
    }

    /// <summary>
    /// Attempts to log in a user with the provided credentials, creates a new API key if successful.
    /// Returns a LoginResult with state flags and user data.
    /// </summary>
    public async Task<LoginResult> LogIn(LoginDataDto loginDataDto,
        CancellationToken ct = default)
    {
        try
        {
            var targetUser =
                await _unitOfWork.ListUserRepository.GetWithoutDetailsByEmailAddressAsync(loginDataDto.EmailAddress,
                    ct);

            if (targetUser is null)
                return new(false, false, null, null, null);

            bool passwordIsCorrect = BCrypt.Net.BCrypt.EnhancedVerify(loginDataDto.Password, targetUser.PasswordHash);

            if (passwordIsCorrect is not true)
                return new(false, true, false, null, null);

            var newApiKeyValue = ApiKey.GenerateKey();

            var newApiKey = await _unitOfWork.ApiKeyRepository.CreateAsync(targetUser.UserId, newApiKeyValue, ct);

            var apiKeySaveResult = await _unitOfWork.SaveChangesAsync(ct);

            if (apiKeySaveResult != 1)
            {
                _logger.LogError(
                    "Failed to create API key for user {UserId} during login. SaveChangesAsync returned {Result}.",
                    targetUser.UserId, apiKeySaveResult);
                return new(false, true, true, false, null);
            }

            var newUserGetDto = new ListUserGetDto(targetUser, newApiKey);

            return new(true, true, true, true, newUserGetDto);
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(AppAuthenticationService), nameof(LogIn));
            throw numberedException;
        }
    }

    /// <summary>
    /// Logs out a user by invalidating the provided API key.
    /// Returns a LogoutResult with state flags.
    /// </summary>
    public async Task<LogoutResult> LogOut(Guid requestingUserId, string providedApiKey, CancellationToken ct = default)
    {
        try
        {
            var targetUserApiKey = await _unitOfWork.ApiKeyRepository.GetByKeyAsync(providedApiKey, ct);

            if (targetUserApiKey is null)
                return new(false, false, null, null);

            _unitOfWork.ApiKeyRepository.Invalidate(targetUserApiKey);

            var checkResult = await _unitOfWork.SaveChangesAsync(ct);

            if (targetUserApiKey.UserId != requestingUserId)
                return new(false, true, false, checkResult == 1);

            return new(checkResult == 1, true, true, checkResult == 1);
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(AppAuthenticationService), nameof(LogOut));
            throw numberedException;
        }
    }

    /// <summary>
    /// Writes an authentication error response to the HTTP context with the specified status code and error enum.
    /// Used internally for custom authentication error handling.
    /// </summary>
    internal static async Task HandleAuthenticationResponseAsync(int httpResponseCode, AuthorizationErrorEnum authEnum,
        HttpContext context)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = httpResponseCode;
        string jsonResponse =
            JsonConvert.SerializeObject(new AuthenticationErrorResponse(authEnum));
        byte[] responseBytes = Encoding.UTF8.GetBytes(jsonResponse);

        await context.Response.Body.WriteAsync(responseBytes);
    }
}