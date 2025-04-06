namespace ShoppingListApi.Model.Database;

public class ItemIdentificationData
{
    public Guid UserId { get; }
    public Guid ShoppingListId { get; }
    public Guid itemId { get; }

    public ItemIdentificationData(Guid userId, Guid shoppingListId, Guid itemId)
    {
        UserId = userId;
        ShoppingListId = shoppingListId;
        this.itemId = itemId;
    }
}