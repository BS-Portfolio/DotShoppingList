namespace ShoppingListApi.Model.Database;

public class CollaboratorRemovalCheck
{
    public Guid ListOwnerId { get; }
    public Guid ShoppingListId { get; }
    public Guid CollaboratorId { get; }
    public Guid RequestingUserId { get; }

    public CollaboratorRemovalCheck(Guid listOwnerId, Guid shoppingListId, Guid collaboratorId, Guid requestingUserId)
    {
        ListOwnerId = listOwnerId;
        ShoppingListId = shoppingListId;
        CollaboratorId = collaboratorId;
        RequestingUserId = requestingUserId;
    }
}