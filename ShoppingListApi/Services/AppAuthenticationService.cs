using System.Text;
using Newtonsoft.Json;
using ShoppingListApi.Configs;
using ShoppingListApi.Enums;
using ShoppingListApi.Exceptions;
using ShoppingListApi.Interfaces.Services;
using ShoppingListApi.Model.ReturnTypes;

namespace ShoppingListApi.Services;

public class AppAuthenticationService(IUnitOfWork unitOfWork, ILogger<AppAuthenticationService> logger)
    : IAppAuthenticationService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<AppAuthenticationService> _logger = logger;

    public async Task<AppAuthenticationResult> AuthenticateAsync(Guid userId, string apiKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var targetUser = await _unitOfWork.ListUserRepository.GetWithoutDetailsByIdAsync(userId, cancellationToken);

            if (targetUser is null)
                return new(false, false, null, null, null);

            var targetApiKey = await _unitOfWork.ApiKeyRepository.GetByKeyAsync(userId, apiKey, cancellationToken);

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
                nameof(AppAuthenticationService), nameof(AuthenticateAsync));
            throw numberedException;
        }
    }

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