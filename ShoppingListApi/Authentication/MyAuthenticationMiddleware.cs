using System.Net.Mime;
using System.Text;
using System.Text.Json.Serialization.Metadata;
using Newtonsoft.Json;
using ShoppingListApi.Attributes;
using ShoppingListApi.Configs;
using ShoppingListApi.Controllers;
using ShoppingListApi.Exceptions;
using ShoppingListApi.Model.ReturnTypes;
using ShoppingListApi.Services;

namespace ShoppingListApi.Authentication;

public class MyAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly DatabaseService _databaseService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MyAuthenticationMiddleware> _logger;

    public MyAuthenticationMiddleware(RequestDelegate next, DatabaseService databaseService,
        IConfiguration configuration,
        ILogger<MyAuthenticationMiddleware> logger)
    {
        _next = next;
        _databaseService = databaseService;
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
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Service not available!");
                return;
            }

            var storedKey = _configuration.GetValue<string>("API-Admin-Key");

            if (storedKey is null)
            {
                await HM.HandleAuthenticationResponse(500, 5, "Service Not Available!", context);
                return;
            }

            if (!storedKey.Equals(extractedMasterKey))
            {
                await HM.HandleAuthenticationResponse(401, 1, "Wrong API key!", context);
                return;
            }

            await _next(context);
            return;
        }


        bool userIdExists = context.Request.Headers.TryGetValue("USER-ID", out var userIdSv);
        bool userKeyExists = context.Request.Headers.TryGetValue("USER-KEY", out var userApiKeySv);

        if (userIdExists is false || userKeyExists is false)
        {
            await HM.HandleAuthenticationResponse(401, 6, "User Credentials Missing!", context);
            return;
        }

        try
        {
            Guid userId = Guid.Parse(userIdSv.ToString());
            string userApiKey = userApiKeySv.ToString();
            var result = await _databaseService.Authenticate(userId, userApiKey);

            if (result.IsAuthenticated)
            {
                await _next(context);
                return;
            }

            if (result.AccountExists is false)
            {
                await HM.HandleAuthenticationResponse(401, 2, "User Account does not exist!", context);
                return;
            }

            if (result.ApiKeyWasEqual is false)
            {
                await HM.HandleAuthenticationResponse(401, 3, "User API Key is not correct", context);
                return;
            }

            if (result.ApiKeyIsValid is false)
            {
                await HM.HandleAuthenticationResponse(401, 4, "User API Key is expired! Login in again!", context);
                return;
            }
        }
        catch (FormatException fEx)
        {
            _logger.LogWithLevel(LogLevel.Error, fEx, "0", fEx.Message,
                nameof(MyAuthenticationMiddleware), nameof(InvokeAsync));
            await HM.HandleAuthenticationResponse(40, 7, "User Credentials are provided in a wrong format!", context);
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