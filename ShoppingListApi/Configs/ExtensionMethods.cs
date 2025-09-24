using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using ShoppingListApi.Enums;
using ShoppingListApi.Model.ReturnTypes;

namespace ShoppingListApi.Configs;

using Microsoft.Extensions.Logging;
using System;

public static class LoggerExtensions
{
    public static void LogWithLevel<T>(this ILogger<T> log, LogLevel logLevel, Exception ex, string errorNumber,
        string message, string className, string methodName)
    {
        switch (logLevel)
        {
            case LogLevel.Error:
                log.LogError(ex,
                    "Error No: {ErrorNumber} | ExceptionClass: {ExceptionClass} | ExceptionMethod: {ExceptionMethod} | Message: {Message} | StackTrace: {StackTrace}",
                    errorNumber, className, methodName, message, ex.StackTrace);
                break;
            case LogLevel.Critical:
                log.LogCritical(ex,
                    "Error No: {ErrorNumber} | ExceptionClass: {ExceptionClass} | ExceptionMethod: {ExceptionMethod} | Message: {Message} | StackTrace: {StackTrace}",
                    errorNumber, className, methodName, message, ex.StackTrace);
                break;
            default:
                return;
        }
    }
}

public static class ControllerExtensions
{
    public static (ActionResult? ActionResult, Guid? RequestingUserId, string? ApiKey) CheckAccess(this ControllerBase controllerBase)
    {
        var requestingUserIdStr = controllerBase.Request.Headers["USER-ID"].FirstOrDefault();
        var apiKey = controllerBase.Request.Headers["USER-KEY"].FirstOrDefault();

        if (string.IsNullOrEmpty(requestingUserIdStr) || string.IsNullOrWhiteSpace(apiKey))
            return (
                controllerBase.Unauthorized(
                    new AuthenticationErrorResponse(AuthorizationErrorEnum.UserCredentialsMissing)),
                null, null);

        var parseSuccessful = Guid.TryParse(requestingUserIdStr, out var requestingUserId);

        if (parseSuccessful is not true)
            return (
                controllerBase.BadRequest(new ResponseResult<object?>(null,
                    "The user Id is provided in an invalid format!")),
                null, null);

        return (null, requestingUserId, apiKey);
    }
}

public static class StringExtensions
{
    public static bool IsEmail(this string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        var emailPattern = @"^[\w\-\.]+@([\w\-]+\.)+[\w\-]{2,}$";
        return Regex.IsMatch(input.Trim(), emailPattern, RegexOptions.IgnoreCase);
    }
}