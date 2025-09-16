using ShoppingListApi.Interfaces.Repositories;
using ShoppingListApi.Interfaces.Services;
using ShoppingListApi.Model.DTOs.Patch;
using ShoppingListApi.Model.DTOs.Post;
using ShoppingListApi.Model.ReturnTypes;

namespace ShoppingListApi.Services;

public class ItemService(IItemRepository itemRepository, IConfiguration configuration, ILogger<ItemService> logger)
    : IItemService
{
    private readonly IItemRepository _itemRepository = itemRepository;
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<ItemService> _logger = logger;

    public IItemRepository ItemRepository => _itemRepository;

    public async Task<AddItemResult> FindShoppingListAndAddItemAsync(
        IListMembershipService listMembershipService, IShoppingListService shoppingListService, Guid userId,
        Guid shoppingListId, ItemPostDto itemPostDto,
        CancellationToken ct = default)
    {
        try
        {
            var maxItemsAmount = _configuration.GetValue<int>("Items_MaxAmount");
            var targetShoppingList =
                await shoppingListService.ShoppingListRepository
                    .GetWithItemsByIdAsync(shoppingListId, ct);

            if (targetShoppingList is null)
                return new(false, null, false, null, null);

            var userRoleEnum = await listMembershipService.GetUserRoleInShoppingListAsync(userId, shoppingListId, ct);

            if (userRoleEnum is null)
                return new(true, false, false, null, null);

            if (targetShoppingList.Items.Count >= maxItemsAmount)
                return new(true, true, false, true, null);

            var newItemId = await _itemRepository.CreateAsync(shoppingListId, itemPostDto, ct);

            if (newItemId is null)
                return new(true, true, false, false, null);

            return new(true, true, true, false, newItemId);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<UpdateRestrictedRecordResult<object?>> FindItemAndUpdateAsync(
        IListMembershipService listMembershipService,
        Guid requestingUserId, Guid shoppingListId, Guid itemId, ItemPatchDto itemPatchDto,
        CancellationToken ct = default)
    {
        try
        {
            var userRoleEnum =
                await listMembershipService.GetUserRoleInShoppingListAsync(requestingUserId, shoppingListId, ct);

            if (userRoleEnum is null)
                return new(null, false, false, null, null);

            var targetItem = await _itemRepository.GetByIdAsync(shoppingListId, itemId, ct);

            if (targetItem is null)
                return new(false, false, true, null, null);

            var updateSuccess = await _itemRepository.UpdateByIdAsync(targetItem, itemPatchDto, ct);

            return new(true, updateSuccess, true, null, null);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<RemoveRestrictedRecordResult> FindItemAndDeleteAsync(IListMembershipService listMembershipService,
        Guid requestingUserId, Guid shoppingListId, Guid itemId, CancellationToken ct = default)
    {
        try
        {
            var userRoleEnum =
                await listMembershipService.GetUserRoleInShoppingListAsync(requestingUserId, shoppingListId, ct);

            if (userRoleEnum is null)
                return new(null, false, false, 0);

            var targetItem = await _itemRepository.GetByIdAsync(shoppingListId, itemId, ct);

            if (targetItem is null) return new(false, true, false, 0);

            var deletionSuccess = await _itemRepository.DeleteAsync(targetItem, ct);

            if (deletionSuccess is false)
                return new(true, true, false, 0);

            return new(true, true, true, 1);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}