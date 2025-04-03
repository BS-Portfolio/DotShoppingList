namespace ShoppingListApi.Model.Database;

public class ShoppingListExistenceCheckData
{
    public Guid UserId { get; }
    public string ShoppingListName { get; }

    public ShoppingListExistenceCheckData(Guid userId, string shoppingListName)
    {
        UserId = userId;
        ShoppingListName = shoppingListName;
    }
}