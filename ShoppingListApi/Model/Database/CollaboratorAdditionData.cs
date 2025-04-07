namespace ShoppingListApi.Model.Database;

public class CollaboratorAdditionData
{
    public Guid ListOwnerId { get; }
    public Guid ShoppingListId { get; }
    public string CollaboratorEmailAddress { get; }
    public Guid RequestingUserId { get; }

    public CollaboratorAdditionData(Guid listOwnerId, Guid shoppingListId, string collaboratorEmailAddress, Guid requestingUserId)
    {
        ListOwnerId = listOwnerId;
        ShoppingListId = shoppingListId;
        CollaboratorEmailAddress = collaboratorEmailAddress;
        RequestingUserId = requestingUserId;
    }
}