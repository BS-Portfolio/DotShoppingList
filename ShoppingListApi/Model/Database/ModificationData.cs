namespace ShoppingListApi.Model.Database;

public class ModificationData<T>
{
    public Guid Identifier { get; }
    public T Payload { get; }

    public ModificationData(Guid identifier, T payload)
    {
        Identifier = identifier;
        Payload = payload;
    }
}