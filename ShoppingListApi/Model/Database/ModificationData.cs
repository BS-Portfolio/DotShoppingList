namespace ShoppingListApi.Model.Database;

public class ModificationData<T1, T2>
{
    public T1 Identifier { get; }
    public T2 Payload { get; }

    public ModificationData(T1 identifier, T2 payload)
    {
        Identifier = identifier;
        Payload = payload;
    }
}