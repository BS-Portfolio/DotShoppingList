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