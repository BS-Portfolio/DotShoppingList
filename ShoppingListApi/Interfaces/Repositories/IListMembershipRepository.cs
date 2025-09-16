using ShoppingListApi.Enums;
using ShoppingListApi.Model.Entity;

namespace ShoppingListApi.Interfaces.Repositories;

public interface IListMembershipRepository
{
    Task<UserRole?> GetUserRoleInShoppingListAsync(Guid listUserId, Guid shoppingListId,
        CancellationToken ct = default);

    Task<List<ShoppingList>> GetAllShoppingListsForUserAsync(Guid listUserId, CancellationToken ct = default);

    Task<List<ShoppingList>> GetAllShoppingListsOwnedByUserAsync(Guid listUserId, CancellationToken ct = default);

    Task<List<ShoppingList>> GetAllCollaboratingShoppingListsForUserAsync(Guid listUserId,
        CancellationToken ct = default);


    Task<bool> AssignUserToShoppingListByUserRoleIdAsync(Guid listUserId, Guid shoppingListId, Guid userRoleId,
        CancellationToken ct = default);

    Task<bool> RemoveListMembershipAsync(ListMembership listMembership, CancellationToken ct = default);
}