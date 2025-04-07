namespace ShoppingListApi.Model.ReturnTypes;

public class ShoppingListAdditionResult
{
    public bool Success { get; private set; }
    public Guid? AddedShoppingListId { get; private set; }
    public bool MaximumNumberOfListsReached { get; private set; }
    public bool? ListAssignmentSuccess { get; private set; }
    public bool NameAlreadyExists { get; }

    public ShoppingListAdditionResult(bool success, Guid? addedShoppingListId, bool maximumNumberOfListsReached,
        bool? listAssignmentSuccess = null, bool nameAlreadyExists = false)
    {
        Success = success;
        AddedShoppingListId = addedShoppingListId;
        MaximumNumberOfListsReached = maximumNumberOfListsReached;
        ListAssignmentSuccess = listAssignmentSuccess;
        NameAlreadyExists = nameAlreadyExists;
    }
}