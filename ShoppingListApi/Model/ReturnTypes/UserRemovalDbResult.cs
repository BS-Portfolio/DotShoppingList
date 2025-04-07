namespace ShoppingListApi.Model.ReturnTypes;

public class UserRemovalDbResult
{
    public bool Success { get; }
    public bool UserExists {get; }
    public int RemovedShoppingListsCount { get; }

    public UserRemovalDbResult(bool success, bool userExists, int removedShoppingListsCount)
    {
        Success = success;
        UserExists = userExists;
        RemovedShoppingListsCount = removedShoppingListsCount;
    }
}