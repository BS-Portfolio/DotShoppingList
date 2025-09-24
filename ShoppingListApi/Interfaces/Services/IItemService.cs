using ShoppingListApi.Model.DTOs.Patch;
using ShoppingListApi.Model.DTOs.Post;
using ShoppingListApi.Model.ReturnTypes;

namespace ShoppingListApi.Interfaces.Services;

public interface IItemService
{
    /// <summary>
    /// Finds a shopping list by ID and adds a new item for the user, checking for role and item count limits.
    /// Returns an AddItemResult with state flags and the new item ID.
    /// </summary>
    Task<AddItemResult> FindShoppingListAndAddItemAsync(Guid userId, Guid shoppingListId, ItemPostDto itemPostDto,
        CancellationToken ct = default);

    /// <summary>
    /// Finds an item by ID in a shopping list and updates it using the provided patch DTO, checking user role.
    /// Returns an UpdateRestrictedRecordResult with state flags.
    /// </summary>
    Task<UpdateRestrictedRecordResult<object?>> FindItemAndUpdateAsync(Guid requestingUserId, Guid shoppingListId,
        Guid itemId, ItemPatchDto itemPatchDto, CancellationToken ct = default);

    /// <summary>
    /// Finds and deletes an item by ID in a shopping list, checking user role.
    /// Returns a RemoveRestrictedRecordResult with state flags and affected records count.
    /// </summary>
    Task<RemoveRestrictedRecordResult> FindItemAndDeleteAsync(Guid requestingUserId, Guid shoppingListId, Guid itemId,
        CancellationToken ct = default);

    /// <summary>
    /// Deletes all items in a shopping list for the requesting user, checking user role.
    /// Returns a RemoveRestrictedRecordResult with state flags and affected records count.
    /// </summary>
    Task<RemoveRestrictedRecordResult> DeleteAllItemsInShoppingListAsync(Guid requestingUserId,
        Guid shoppingListId,
        CancellationToken ct = default);
}