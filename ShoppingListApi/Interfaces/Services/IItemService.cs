using ShoppingListApi.Model.DTOs.Patch;
using ShoppingListApi.Model.DTOs.Post;
using ShoppingListApi.Model.ReturnTypes;

namespace ShoppingListApi.Interfaces.Services;

public interface IItemService
{
    Task<AddItemResult> FindShoppingListAndAddItemAsync(Guid userId, Guid shoppingListId, ItemPostDto itemPostDto,
        CancellationToken ct = default);

    Task<UpdateRestrictedRecordResult<object?>> FindItemAndUpdateAsync(Guid requestingUserId, Guid shoppingListId,
        Guid itemId, ItemPatchDto itemPatchDto, CancellationToken ct = default);

    Task<RemoveRestrictedRecordResult> FindItemAndDeleteAsync(Guid requestingUserId, Guid shoppingListId, Guid itemId,
        CancellationToken ct = default);

    Task<RemoveRestrictedRecordResult> DeleteAllItemsInShoppingListAsync(Guid requestingUserId,
        Guid shoppingListId,
        CancellationToken ct = default);
}