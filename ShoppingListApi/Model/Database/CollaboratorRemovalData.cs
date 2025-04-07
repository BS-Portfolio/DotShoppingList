namespace ShoppingListApi.Model.Database;

public class CollaboratorRemovalData
{
    public Guid ShoppingListId { get; }
    public Guid UserId { get; }

    public CollaboratorRemovalData(Guid shoppingListId, Guid userId)
    {
        ShoppingListId = shoppingListId;
        UserId = userId;
    }
}