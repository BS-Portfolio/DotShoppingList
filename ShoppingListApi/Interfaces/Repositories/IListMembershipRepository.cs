using ShoppingListApi.Enums;
using ShoppingListApi.Model.Entity;

namespace ShoppingListApi.Interfaces.Repositories;

public interface IListMembershipRepository
{
    Task<UserRole?> GetUserRoleObjInShoppingListAsync(Guid listUserId, Guid shoppingListId,
        CancellationToken ct = default);

    Task<UserRoleEnum?> GetUserRoleEnumInShoppingListAsync(Guid listUserId, Guid shoppingListId,
        CancellationToken ct = default);

    Task<ListMembership?> GetListMembershipByCompositePkAsync(Guid shoppingListId, Guid listUserId,
        CancellationToken ct = default);

    Task<List<Guid>> GetAllShoppingListIdsForUserAsync(Guid listUserId, CancellationToken ct = default);

    // Includes ShoppingList and UserRole
    Task<List<ListMembership>>
        GetAllListMembershipsWithDetailsForOwnerByUserAsync(Guid listUserId, CancellationToken ct = default);

    // Includes ShoppingList and UserRole
    Task<List<ListMembership>> GetAllListMembershipsWithDetailsForNotOwnerByUserAsync(Guid listUserId,
        CancellationToken ct = default);

    // Does not include UserRole and ShoppingList and ListUser
    Task<List<ListMembership>> GetAllMembershipsByShoppingListIdAsync(Guid shoppingListId,
        CancellationToken ct = default);

    Task<List<ShoppingList>> GetAllCollaboratingShoppingListsForUserAsync(Guid listUserId,
        CancellationToken ct = default);

    Task<List<ListMembership>> GetAllUsersInShoppingListAsync(Guid shoppingListId, CancellationToken ct = default);

    Task<ListUser?> GetShoppingListOwner(Guid shoppingListId, CancellationToken ct = default);

    Task<List<ListUser>> GetShoppingListCollaborators(Guid shoppingListId, CancellationToken ct = default);

    Task<ListMembership> AssignUserToShoppingListByUserRoleIdAsync(Guid listUserId, Guid shoppingListId,
        Guid userRoleId,
        CancellationToken ct = default);

    void Delete(ListMembership listMembership);
    void DeleteBatch(List<ListMembership> listMembership);
}