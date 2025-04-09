namespace ShoppingListApi.Model.Database;

public class ShoppingListRemovalResult
{
    public bool Success { get; }
    public bool Exists { get; }
    public bool? AccessGranted { get; }

    public ShoppingListRemovalResult(bool success, bool exists, bool? accessGranted = null)
    {
        Success = success;
        Exists = exists;
        AccessGranted = accessGranted;
    }
}