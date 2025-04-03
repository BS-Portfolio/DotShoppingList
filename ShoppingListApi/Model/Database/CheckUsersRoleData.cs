namespace ShoppingListApi.Model.Database;

public class CheckUsersRoleData
{
    public Guid UserId { get; set; }
    public Guid ShoppingListId { get; set; }

    public CheckUsersRoleData(Guid userId, Guid shoppingListId)
    {
        UserId = userId;
        ShoppingListId = shoppingListId;
    }
}