namespace ShoppingListApi.Configs;

using Microsoft.Extensions.Logging;
using System;

public static class LoggerExtensions
{
    // Generic method for logging at any level with class and method names passed as parameters
    public static void LogWithLevel<T>(this ILogger<T> log, LogLevel logLevel, Exception ex, string errorNumber, string message, string className, string methodName)
    {
        // Structured logging depending on the level
        switch (logLevel)
        {
            case LogLevel.Error:
                log.LogError(ex, "Error No: {ErrorNumber} | ExceptionClass: {ExceptionClass} | ExceptionMethod: {ExceptionMethod} | Message: {Message} | StackTrace: {StackTrace}",
                    errorNumber, className, methodName, message, ex?.StackTrace);
                break;
            case LogLevel.Critical:
                log.LogCritical(ex, "Error No: {ErrorNumber} | ExceptionClass: {ExceptionClass} | ExceptionMethod: {ExceptionMethod} | Message: {Message} | StackTrace: {StackTrace}",
                    errorNumber, className, methodName, message, ex?.StackTrace);
                break;
            default:
                return;
        }
    }
}
