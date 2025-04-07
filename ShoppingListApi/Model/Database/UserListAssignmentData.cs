using ShoppingListApi.Enums;

namespace ShoppingListApi.Model.Database;

public class UserListAssignmentData
{
    public Guid UserId { get; }
    public Guid ShoppingListId { get; }
    public UserRoleEnum UserRole { get; }

    public UserListAssignmentData(Guid userId, Guid shoppingListId, UserRoleEnum userRole)
    {
        UserId = userId;
        ShoppingListId = shoppingListId;
        UserRole = userRole;
    }
}