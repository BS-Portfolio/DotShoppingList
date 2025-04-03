namespace ShoppingListApi.Model.Database;

public class UserListAssignmentData
{
    public Guid UserId { get; }
    public Guid ShoppingListId { get; }
    public Guid UserRoleId { get; }

    public UserListAssignmentData(Guid userId, Guid shoppingListId, Guid userRoleId)
    {
        UserId = userId;
        ShoppingListId = shoppingListId;
        UserRoleId = userRoleId;
    }
}