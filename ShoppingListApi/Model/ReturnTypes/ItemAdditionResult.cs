namespace ShoppingListApi.Model.ReturnTypes;

public class ItemAdditionResult
{
    public bool Success { get; private set; }
    public Guid? ItemId { get; private set; }
    public bool MaximumCountReached { get; private set; }
    public bool AccessGranted { get; }

    public ItemAdditionResult(bool success, bool maximumCountReached, Guid? itemId, bool accessGranted = true)
    {
        Success = success;
        MaximumCountReached = maximumCountReached;
        ItemId = itemId;
        AccessGranted = accessGranted;
    }
}