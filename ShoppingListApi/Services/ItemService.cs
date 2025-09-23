using ShoppingListApi.Configs;
using ShoppingListApi.Exceptions;
using ShoppingListApi.Interfaces.Services;
using ShoppingListApi.Model.DTOs.Patch;
using ShoppingListApi.Model.DTOs.Post;
using ShoppingListApi.Model.ReturnTypes;

namespace ShoppingListApi.Services;

public class ItemService(IUnitOfWork unitOfWork, IConfiguration configuration, ILogger<ItemService> logger)
    : IItemService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<ItemService> _logger = logger;

    
    public async Task<AddItemResult> FindShoppingListAndAddItemAsync(Guid userId, Guid shoppingListId,
        ItemPostDto itemPostDto, CancellationToken ct = default)
    {
        try
        {
            var maxItemsAmount = _configuration.GetValue<int>("Items_MaxAmount");
            if (maxItemsAmount <= 0) maxItemsAmount = 20;

            var userRole =
                await _unitOfWork.ListMembershipRepository.GetUserRoleObjInShoppingListAsync(userId, shoppingListId,
                    ct);

            if (userRole is null)
                return new(false, null, false, null, null);

            var targetShoppingList = await _unitOfWork.ShoppingListRepository.GetWithItemsByIdAsync(shoppingListId, ct);

            if (targetShoppingList is null)
                return new(false, false, true, null, null);

            if (targetShoppingList.Items.Count >= maxItemsAmount)
                return new(false, true, true, true, null);

            var newItemId = await _unitOfWork.ItemRepository.CreateAsync(shoppingListId, itemPostDto, ct);

            var checkResult = await _unitOfWork.SaveChangesAsync(ct);

            if (checkResult != 1)
                return new(false, true, true, false, null);

            return new(true, true, true, false, newItemId);
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(ItemService), nameof(FindShoppingListAndAddItemAsync));
            throw numberedException;
        }
    }

    public async Task<UpdateRestrictedRecordResult<object?>> FindItemAndUpdateAsync(
        Guid requestingUserId, Guid shoppingListId, Guid itemId, ItemPatchDto itemPatchDto,
        CancellationToken ct = default)
    {
        try
        {
            var userRole =
                await _unitOfWork.ListMembershipRepository.GetUserRoleObjInShoppingListAsync(requestingUserId,
                    shoppingListId, ct);

            if (userRole is null)
                return new(null, false, false, null, null);

            var targetItem = await _unitOfWork.ItemRepository.GetByIdAsync(shoppingListId, itemId, ct);

            if (targetItem is null)
                return new(false, false, true, null, null);

            _unitOfWork.ItemRepository.UpdateById(targetItem, itemPatchDto);

            var checkResult = await _unitOfWork.SaveChangesAsync(ct);

            if (checkResult != 1)
                return new(true, false, true, null, null);

            return new(true, true, true, null, null);
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(ItemService), nameof(FindItemAndUpdateAsync));
            throw numberedException;
        }
    }

    public async Task<RemoveRestrictedRecordResult> FindItemAndDeleteAsync(Guid requestingUserId, Guid shoppingListId,
        Guid itemId, CancellationToken ct = default)
    {
        try
        {
            var userRole =
                await _unitOfWork.ListMembershipRepository.GetUserRoleObjInShoppingListAsync(requestingUserId,
                    shoppingListId, ct);

            if (userRole is null)
                return new(null, false, false, 0);

            var targetItem = await _unitOfWork.ItemRepository.GetByIdAsync(shoppingListId, itemId, ct);

            if (targetItem is null) return new(false, true, false, 0);

            _unitOfWork.ItemRepository.Delete(targetItem);

            var checkResult = await _unitOfWork.SaveChangesAsync(ct);

            if (checkResult != 1)
                return new(true, true, false, 0);

            return new(true, true, true, 1);
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(ItemService), nameof(FindItemAndDeleteAsync));
            throw numberedException;
        }
    }

    public async Task<RemoveRestrictedRecordResult> DeleteAllItemsInShoppingListAsync(Guid requestingUserId,
        Guid shoppingListId,
        CancellationToken ct = default)
    {
        try
        {
            var userRole =
                await _unitOfWork.ListMembershipRepository.GetUserRoleObjInShoppingListAsync(requestingUserId,
                    shoppingListId, ct);

            if (userRole is null)
                return new(null, false, false, 0);

            var targetShoppingList = await _unitOfWork.ShoppingListRepository.GetWithItemsByIdAsync(shoppingListId, ct);

            if (targetShoppingList is null)
                return new(false, true, false, 0);

            var itemsCount = targetShoppingList.Items.Count;

            if (itemsCount == 0)
                return new(true, true, true, 0);

            _unitOfWork.ItemRepository.DeleteBatch(targetShoppingList.Items.ToList());

            var checkResult = await _unitOfWork.SaveChangesAsync(ct);

            if (checkResult != itemsCount)
                return new(true, true, false, checkResult);

            return new(true, true, true, checkResult);
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(ItemService), nameof(DeleteAllItemsInShoppingListAsync));
            throw numberedException;
        }
    }
}