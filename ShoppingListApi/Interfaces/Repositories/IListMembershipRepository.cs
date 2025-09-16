using ShoppingListApi.Enums;
using ShoppingListApi.Model.Entity;

namespace ShoppingListApi.Interfaces.Repositories;

public interface IListMembershipRepository
{
    Task<UserRole?> GetUserRoleInShoppingListAsync(Guid listUserId, Guid shoppingListId,
        CancellationToken ct = default);

    Task<ListMembership?> GetListMembershipByCompositePkAsync(Guid listUserId, Guid shoppingListId,
        CancellationToken ct = default);
    Task<List<Guid>> GetAllShoppingListIdsForUserAsync(Guid listUserId, CancellationToken ct = default);

    Task<List<ShoppingList>> GetAllShoppingListsOwnedByUserAsync(Guid listUserId, CancellationToken ct = default);

    Task<List<ShoppingList>> GetAllCollaboratingShoppingListsForUserAsync(Guid listUserId,
        CancellationToken ct = default);

    Task<List<ListMembership>> GetAllUsersInShoppingListAsync(Guid shoppingListId, CancellationToken ct = default);

    Task<ListUser?> GetShoppingListOwner(Guid shoppingListId, CancellationToken ct = default);

    Task<List<ListUser>> GetShoppingListCollaborators(Guid shoppingListId, CancellationToken ct = default);

    Task<bool> AssignUserToShoppingListByUserRoleIdAsync(Guid listUserId, Guid shoppingListId, Guid userRoleId,
        CancellationToken ct = default);

    Task<bool> RemoveListMembershipAsync(ListMembership listMembership, CancellationToken ct = default);
}