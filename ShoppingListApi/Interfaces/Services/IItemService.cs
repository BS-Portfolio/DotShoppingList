using ShoppingListApi.Interfaces.Repositories;
using ShoppingListApi.Model.DTOs.Patch;
using ShoppingListApi.Model.DTOs.Post;
using ShoppingListApi.Model.Entity;
using ShoppingListApi.Model.ReturnTypes;

namespace ShoppingListApi.Interfaces.Services;

public interface IItemService
{
    IItemRepository ItemRepository { get; }

    Task<AddItemResult> FindShoppingListAndAddItemAsync(IListMembershipService listMembershipService,
        IShoppingListService shoppingListService, Guid userId,
        Guid shoppingListId, ItemPostDto itemPostDto,
        CancellationToken ct = default);

    Task<UpdateRestrictedRecordResult<object?>> FindItemAndUpdateAsync(IListMembershipService listMembershipService,
        Guid requestingUserId, Guid shoppingListId, Guid itemId,
        ItemPatchDto itemPatchDto,
        CancellationToken ct = default);

    Task<RemoveRestrictedRecordResult> FindItemAndDeleteAsync(IListMembershipService listMembershipService, Guid requestingUserId,
        Guid shoppingListId, Guid itemId,
        CancellationToken ct = default);
}