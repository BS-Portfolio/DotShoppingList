using ShoppingListApi.Model.DTOs.Get;
using ShoppingListApi.Model.DTOs.Post;
using ShoppingListApi.Model.Entity;
using ShoppingListApi.Model.ReturnTypes;

namespace ShoppingListApi.Interfaces.Services;

public interface IShoppingListService
{
    /// <summary>
    /// Retrieves all shopping lists for a user, including items, owner, and collaborators. Checks access via memberships.
    /// Returns a FetchRestrictedRecordResult with the list DTOs and access flags.
    /// </summary>
    Task<FetchRestrictedRecordResult<List<ShoppingListGetDto>?>> CheckAccessAndGetAllShoppingListsForUser(
        Guid requestingUserId, CancellationToken ct = default);

    /// <summary>
    /// Retrieves a single shopping list for a user by ID, including items, owner, and collaborators. Checks access via membership and role.
    /// Returns a FetchRestrictedRecordResult with the DTO and access flags.
    /// </summary>
    Task<FetchRestrictedRecordResult<ShoppingListGetDto?>> CheckAccessAndGetShoppingListByIdAsync(
        Guid requestingUserId, Guid shoppingListId, CancellationToken ct = default);

    /// <summary>
    /// Creates a new shopping list for a user, checking for name conflicts and maximum allowed lists.
    /// Returns a ShoppingListAdditionResult with state flags and the new list.
    /// </summary>
    Task<ShoppingListAdditionResult> CheckConflictAndCreateShoppingListAsync(
        Guid requestingUserId, ShoppingListPostDto shoppingListPostDto,
        CancellationToken ct = default);

    /// <summary>
    /// Updates the name of a shopping list for a user, checking access and name conflicts.
    /// Returns an UpdateRestrictedRecordResult with state flags and the updated list.
    /// </summary>
    Task<UpdateRestrictedRecordResult<ShoppingList?>> CheckAccessAndUpdateShoppingListNameAsync(
        Guid requestingUserId, Guid shoppingListId, ShoppingListPostDto shoppingListPostDto,
        CancellationToken ct = default);

    /// <summary>
    /// Deletes a shopping list for a user, checking access and existence. May involve cascading deletes.
    /// Returns a RemoveRestrictedRecordResult with state flags and affected records count.
    /// </summary>
    Task<RemoveRestrictedRecordResult> CheckAccessAndDeleteShoppingListAsync(
        Guid requestingUserId, Guid shoppingListId, CancellationToken ct = default);

    /// <summary>
    /// Deletes a shopping list as an app admin, checking existence. May involve cascading deletes.
    /// Returns a RemoveRecordResult with state flags and affected records count.
    /// </summary>
    Task<RemoveRecordResult> CheckExistenceAndDeleteShoppingListAsAppAdminAsync(
        Guid shoppingListId, CancellationToken ct = default);
}