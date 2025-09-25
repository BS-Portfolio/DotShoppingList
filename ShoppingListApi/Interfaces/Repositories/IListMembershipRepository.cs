using ShoppingListApi.Enums;
using ShoppingListApi.Model.Entity;

namespace ShoppingListApi.Interfaces.Repositories;

public interface IListMembershipRepository
{
    /// <summary>
    /// Retrieves the UserRole object for a user in a specific shopping list.
    /// Returns null if no membership is found.
    /// </summary>
    Task<UserRole?> GetUserRoleObjInShoppingListAsync(Guid listUserId, Guid shoppingListId,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves the UserRoleEnum for a user in a specific shopping list.
    /// Returns null if no membership or role is found.
    /// </summary>
    Task<UserRoleEnum?> GetUserRoleEnumInShoppingListAsync(Guid listUserId, Guid shoppingListId,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves a ListMembership by its composite primary key (shoppingListId, listUserId).
    /// Returns null if not found.
    /// </summary>
    Task<ListMembership?> GetListMembershipByCompositePkAsync(Guid shoppingListId, Guid listUserId,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves all shopping list IDs for a given user.
    /// </summary>
    Task<List<Guid>> GetAllShoppingListIdsForUserAsync(Guid listUserId, CancellationToken ct = default);

    /// <summary>
    /// Retrieves all ListMemberships for a user where the user is the list owner, including ShoppingList and UserRole details.
    /// </summary>
    Task<List<ListMembership>> GetAllListMembershipsWithDetailsForOwnerByUserAsync(Guid listUserId,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves all ListMemberships for a user where the user is not the list owner, including ShoppingList and UserRole details.
    /// </summary>
    Task<List<ListMembership>> GetAllListMembershipsWithDetailsForNotOwnerByUserAsync(Guid listUserId,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves all ListMemberships for a shopping list without including UserRole, ShoppingList, or ListUser details.
    /// </summary>
    Task<List<ListMembership>> GetAllMembershipsWithoutCascadingInfoByShoppingListIdAsync(Guid shoppingListId,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves all ListMemberships for a shopping list, including User and UserRole details.
    /// </summary>
    Task<List<ListMembership>> GetAllMembershipsWithCascadingInfoByShoppingListIdAsync(Guid shoppingListId,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves all ListMemberships for multiple shopping lists, including User and UserRole details.
    /// </summary>
    Task<List<ListMembership>> GetAllMembershipsWithCascadingInfoByShoppingListIdsAsync(List<Guid> shoppingListId,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves all ListMemberships for a user, including ShoppingList (with Items), UserRole, and User details.
    /// </summary>
    Task<List<ListMembership>> GetAllMembershipsWithCascadingInfoByUserIdAsync(Guid listUserId,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves all shopping lists where the user is a collaborator.
    /// </summary>
    Task<List<ShoppingList>> GetAllCollaboratingShoppingListsForUserAsync(Guid listUserId,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves all users in a shopping list, including UserRole and User details.
    /// </summary>
    Task<List<ListMembership>> GetAllUsersInShoppingListAsync(Guid shoppingListId, CancellationToken ct = default);

    /// <summary>
    /// Retrieves the owner (ListUser) of a shopping list. Throws an exception if multiple owners are found.
    /// Returns null if no owner is found.
    /// </summary>
    Task<ListUser?> GetShoppingListOwner(Guid shoppingListId, CancellationToken ct = default);

    /// <summary>
    /// Retrieves all collaborators (ListUser) in a shopping list.
    /// </summary>
    Task<List<ListUser>> GetShoppingListCollaborators(Guid shoppingListId, CancellationToken ct = default);

    /// <summary>
    /// Checks if a user owns a shopping list with a given name, optionally excluding a specific list ID.
    /// Returns the ShoppingList if found, otherwise null.
    /// </summary>
    Task<ShoppingList?> OwnsShoppingListWithNameAsync(Guid userId, string listName, Guid? excludeListId,
        CancellationToken ct = default);

    /// <summary>
    /// Assigns a user to a shopping list with a specific user role ID. Does not save changes to the database.
    /// Returns the new ListMembership.
    /// </summary>
    Task<ListMembership> AssignUserToShoppingListByUserRoleIdAsync(Guid listUserId, Guid shoppingListId,
        Guid userRoleId,
        CancellationToken ct = default);

    /// <summary>
    /// Removes the specified ListMembership from the database context. Does not save changes.
    /// </summary>
    void Delete(ListMembership listMembership);

    /// <summary>
    /// Removes a batch of ListMemberships from the database context. Does not save changes.
    /// </summary>
    void DeleteBatch(List<ListMembership> listMembership);
}