namespace ShoppingListApi.Exceptions;

public class NoContentFoundException<T> : NumberedException
{
    public T Identifier { get; }

    public NoContentFoundException(T identifier)
    {
        Identifier = identifier;
    }

    public NoContentFoundException(string? message, T identifier) : base(message)
    {
        Identifier = identifier;
    }

    public NoContentFoundException(string? message, Exception? innerException, T identifier) : base(message,
        innerException)
    {
        Identifier = identifier;
    }
}