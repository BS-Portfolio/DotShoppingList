namespace ShoppingListApi.Model.ReturnTypes;

public class ShoppingListAdditionResult
{
    public bool Success { get; private set; }
    public Guid? AddedShoppingListId { get; private set; }
    public bool MaximumNumberOfListsReached { get; private set; }
    public bool? ListAssignmentSuccess { get; private set; }

    public ShoppingListAdditionResult(bool success, Guid? addedShoppingListId, bool maximumNumberOfListsReached,
        bool? listAssignmentSuccess = null)
    {
        Success = success;
        AddedShoppingListId = addedShoppingListId;
        MaximumNumberOfListsReached = maximumNumberOfListsReached;
        ListAssignmentSuccess = listAssignmentSuccess;
    }
}