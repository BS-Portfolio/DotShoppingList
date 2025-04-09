using System.Net.Mime;
using System.Text;
using System.Text.Json.Serialization.Metadata;
using Newtonsoft.Json;
using ShoppingListApi.Attributes;
using ShoppingListApi.Configs;
using ShoppingListApi.Enums;
using ShoppingListApi.Exceptions;
using ShoppingListApi.Model.ReturnTypes;
using ShoppingListApi.Services;

namespace ShoppingListApi.Authentication;

public class MyAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly MyAuthenticationService _myAuthenticationService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MyAuthenticationMiddleware> _logger;

    public MyAuthenticationMiddleware(RequestDelegate next, MyAuthenticationService myAuthenticationService,
        IConfiguration configuration,
        ILogger<MyAuthenticationMiddleware> logger)
    {
        _next = next;
        _myAuthenticationService = myAuthenticationService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();

        if (endpoint is null)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Not authorized!");
            return;
        }

        if (endpoint.Metadata.GetMetadata<PublicEndpointAttribute>() != null)
        {
            await _next(context);
            return;
        }

        if (endpoint.Metadata.GetMetadata<AdminEndpointAttribute>() != null)
        {
            if (!context.Request.Headers.TryGetValue("X-API-KEY", out var extractedMasterKey))
            {
                await HM.HandleAuthenticationResponseAsync(401, AuthorizationErrorEnum.ApiKeyIsMissing, context);
                return;
            }

            var storedKey = _configuration.GetValue<string>("API-Admin-Key");

            if (storedKey is null)
            {
                await HM.HandleAuthenticationResponseAsync(500, AuthorizationErrorEnum.ServiceNotAvailable, context);
                return;
            }

            if (!storedKey.Equals(extractedMasterKey))
            {
                await HM.HandleAuthenticationResponseAsync(401, AuthorizationErrorEnum.WrongApiKey, context);
                return;
            }

            await _next(context);
            return;
        }


        bool userIdExists = context.Request.Headers.TryGetValue("USER-ID", out var userIdSv);
        bool userKeyExists = context.Request.Headers.TryGetValue("USER-KEY", out var userApiKeySv);

        if (userIdExists is false || userKeyExists is false)
        {
            await HM.HandleAuthenticationResponseAsync(401, AuthorizationErrorEnum.UserCredentialsMissing, context);
            return;
        }

        try
        {
            Guid userId = Guid.Parse(userIdSv.ToString());
            string userApiKey = userApiKeySv.ToString();
            var result = await _myAuthenticationService.AuthenticateAsync(userId, userApiKey);

            if (result.IsAuthenticated)
            {
                await _next(context);
                return;
            }

            if (result.AccountExists is false)
            {
                await HM.HandleAuthenticationResponseAsync(401, AuthorizationErrorEnum.UserAccountNotFound, context);
                return;
            }

            if (result.ApiKeyWasEqual is false)
            {
                await HM.HandleAuthenticationResponseAsync(401, AuthorizationErrorEnum.InvalidUserApiKey, context);
                return;
            }

            if (result.ApiKeyIsValid is false)
            {
                await HM.HandleAuthenticationResponseAsync(401, AuthorizationErrorEnum.ExpiredApiKey, context);
                return;
            }
        }
        catch (FormatException fEx)
        {
            _logger.LogWithLevel(LogLevel.Error, fEx, "0", fEx.Message,
                nameof(MyAuthenticationMiddleware), nameof(InvokeAsync));
            await HM.HandleAuthenticationResponseAsync(40, AuthorizationErrorEnum.WrongFormat, context);
        }
        catch (NumberedException nEx)
        {
            _logger.LogWithLevel(LogLevel.Error, nEx, nEx.ErrorNumber, nEx.Message,
                nameof(MyAuthenticationMiddleware), nameof(InvokeAsync));
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync("Service not available!");
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(MyAuthenticationMiddleware), nameof(InvokeAsync));
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync("Service not available!");
        }
    }
}