namespace ShoppingListApi.Exceptions;

public class RecordNotFoundException<T>: NumberedException
{
    public T Identifier { get; }

    public RecordNotFoundException(T identifier)
    {
        Identifier = identifier;
    }

    public RecordNotFoundException(string? message, T identifier) : base(message)
    {
        Identifier = identifier;
    }

    public RecordNotFoundException(string? message, Exception? innerException, T identifier) : base(message, innerException)
    {
        Identifier = identifier;
    }
}