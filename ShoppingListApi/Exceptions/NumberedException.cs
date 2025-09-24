namespace ShoppingListApi.Exceptions;

/// <summary>
/// Custom exception that generates a unique error number and timestamp for each instance.
/// - Stores the error number and UTC date/time of the exception.
/// - Error number is based on the Unix timestamp and milliseconds for uniqueness.
/// - Supports standard exception constructors for message and inner exception.
/// </summary>
public class NumberedException : Exception
{
    public string ErrorNumber { get; }
    public DateTimeOffset ErrorDate { get; }

    public NumberedException()
    {
        ErrorDate = DateTimeOffset.UtcNow;
        ErrorNumber = GenerateErrorNumber(ErrorDate);
    }

    public NumberedException(string? message) : base(message)
    {
        ErrorDate = DateTimeOffset.UtcNow;
        ErrorNumber = GenerateErrorNumber(ErrorDate);
    }

    public NumberedException(string? message, Exception? innerException) : base(message, innerException)
    {
        ErrorDate = DateTimeOffset.UtcNow;
        ErrorNumber = GenerateErrorNumber(ErrorDate);
    }

    public NumberedException(Exception exception) : base(exception.Message, exception)
    {
        ErrorDate = DateTimeOffset.UtcNow;
        ErrorNumber = GenerateErrorNumber(ErrorDate);
    }
    private static string GenerateErrorNumber(DateTimeOffset timestamp)
    {
        long unixTimestamp = timestamp.ToUnixTimeSeconds();

        long uniqueErrorNumber = unixTimestamp * 1000 + timestamp.Millisecond;

        return uniqueErrorNumber.ToString();
    }
}