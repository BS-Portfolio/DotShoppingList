namespace ShoppingListApi.Model.ReturnTypes;

public record ShoppingListAdditionResult
{
    public bool Success { get; private set; }
    public Guid? AddedShoppingListId { get; private set; }
    public bool? MaximumNumberOfListsReached { get; private set; }
    public bool? ListAssignmentSuccess { get; private set; }
    public bool? NameAlreadyExists { get; private set; }
    public bool? AccessGranted { get; private set; }

    public ShoppingListAdditionResult(bool success, Guid? addedShoppingListId, bool? maximumNumberOfListsReached,
        bool? listAssignmentSuccess = null, bool? nameAlreadyExists = false, bool? accessGranted = null)
    {
        Success = success;
        AddedShoppingListId = addedShoppingListId;
        MaximumNumberOfListsReached = maximumNumberOfListsReached;
        ListAssignmentSuccess = listAssignmentSuccess;
        NameAlreadyExists = nameAlreadyExists;
        AccessGranted = accessGranted;
    }
}