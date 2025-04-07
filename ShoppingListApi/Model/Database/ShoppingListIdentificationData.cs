namespace ShoppingListApi.Model.Database;

public class ShoppingListIdentificationData
{
    public Guid UserId { get; }
    public Guid ShoppingListId { get; }

    public ShoppingListIdentificationData(Guid userId, Guid shoppingListId)
    {
        UserId = userId;
        ShoppingListId = shoppingListId;
    }
}