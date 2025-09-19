using ShoppingListApi.Attributes;
using ShoppingListApi.Configs;
using ShoppingListApi.Enums;
using ShoppingListApi.Exceptions;
using ShoppingListApi.Interfaces.Services;
using ShoppingListApi.Services;

namespace ShoppingListApi.Authentication;

public class AppAuthenticationMiddleware(
    RequestDelegate next,
    IServiceProvider serviceProvider,
    IConfiguration configuration,
    ILogger<AppAuthenticationMiddleware> logger)
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public async Task InvokeAsync(HttpContext context)
    {
        using var scope = _serviceProvider.CreateScope();
        var appAuthenticationService =
            scope.ServiceProvider.GetRequiredService<IAppAuthenticationService>();

        var endpoint = context.GetEndpoint();

        if (endpoint is null)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Not authorized!");
            return;
        }

        if (endpoint.Metadata.GetMetadata<PublicEndpointAttribute>() != null)
        {
            await next(context);
            return;
        }

        if (endpoint.Metadata.GetMetadata<AdminEndpointAttribute>() != null)
        {
            if (!context.Request.Headers.TryGetValue("X-API-KEY", out var extractedMasterKey))
            {
                await AppAuthenticationService.HandleAuthenticationResponseAsync(401,
                    AuthorizationErrorEnum.ApiKeyIsMissing, context);
                return;
            }

            var storedKey = configuration.GetValue<string>("API-Admin-Key");

            if (storedKey is null)
            {
                await AppAuthenticationService.HandleAuthenticationResponseAsync(500,
                    AuthorizationErrorEnum.ServiceNotAvailable, context);
                return;
            }

            if (!storedKey.Equals(extractedMasterKey))
            {
                await AppAuthenticationService.HandleAuthenticationResponseAsync(401,
                    AuthorizationErrorEnum.WrongApiKey, context);
                return;
            }

            await next(context);
            return;
        }


        bool userIdExists = context.Request.Headers.TryGetValue("USER-ID", out var userIdSv);
        bool userKeyExists = context.Request.Headers.TryGetValue("USER-KEY", out var userApiKeySv);

        if (userIdExists is false || userKeyExists is false)
        {
            await AppAuthenticationService.HandleAuthenticationResponseAsync(401,
                AuthorizationErrorEnum.UserCredentialsMissing, context);
            return;
        }

        try
        {
            Guid userId = Guid.Parse(userIdSv.ToString());
            string userApiKey = userApiKeySv.ToString();
            var result = await appAuthenticationService.AuthenticateAsync(userId, userApiKey);

            if (result.IsAuthenticated is not true)
            {
                if (result.AccountExists is not true)
                {
                    await AppAuthenticationService.HandleAuthenticationResponseAsync(401,
                        AuthorizationErrorEnum.UserAccountNotFound, context);
                    return;
                }

                if (result.ApiKeyExists is not true)
                {
                    await AppAuthenticationService.HandleAuthenticationResponseAsync(401,
                        AuthorizationErrorEnum.InvalidUserApiKey, context);
                    return;
                }

                if (result.ApiKeyIsValid is not true)
                {
                    await AppAuthenticationService.HandleAuthenticationResponseAsync(401,
                        AuthorizationErrorEnum.InvalidUserApiKey, context);
                    return;
                }

                if (result.ApiKeyIsExpired is true)
                {
                    await AppAuthenticationService.HandleAuthenticationResponseAsync(401,
                        AuthorizationErrorEnum.ExpiredApiKey, context);
                    return;
                }

                await AppAuthenticationService.HandleAuthenticationResponseAsync(500,
                    AuthorizationErrorEnum.ServiceNotAvailable, context);
            }

            await next(context);
        }
        catch (FormatException fEx)
        {
            logger.LogWithLevel(LogLevel.Error, fEx, "0", fEx.Message,
                nameof(AppAuthenticationMiddleware), nameof(InvokeAsync));
            await AppAuthenticationService.HandleAuthenticationResponseAsync(400, AuthorizationErrorEnum.WrongFormat,
                context);
        }
        catch (NumberedException nEx)
        {
            logger.LogWithLevel(LogLevel.Error, nEx, nEx.ErrorNumber, nEx.Message,
                nameof(AppAuthenticationMiddleware), nameof(InvokeAsync));
            await AppAuthenticationService.HandleAuthenticationResponseAsync(500,
                AuthorizationErrorEnum.ServiceNotAvailable,
                context);
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(AppAuthenticationMiddleware), nameof(InvokeAsync));
            await AppAuthenticationService.HandleAuthenticationResponseAsync(500,
                AuthorizationErrorEnum.ServiceNotAvailable,
                context);
        }
    }
}