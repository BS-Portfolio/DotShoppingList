using Microsoft.EntityFrameworkCore;
using ShoppingListApi.Data.Contexts;
using ShoppingListApi.Interfaces.Repositories;
using ShoppingListApi.Model.DTOs.Patch;
using ShoppingListApi.Model.DTOs.Post;
using ShoppingListApi.Model.Entity;

namespace ShoppingListApi.Repositories;

public class ItemRepository(AppDbContext appDbContext) : IItemRepository
{
    private readonly AppDbContext _appDbContext = appDbContext;


    /// <summary>
    /// Retrieves an item by its ID and shopping list ID.
    /// Returns null if not found.
    /// </summary>
    public async Task<Item?> GetByIdAsync(Guid shoppingListId, Guid itemId, CancellationToken ct = default)
    {
        return await _appDbContext.Items.FirstOrDefaultAsync(item =>
            item.ShoppingListId == shoppingListId && item.ItemId == itemId, ct);
    }

    /// <summary>
    /// Retrieves all items for a given shopping list ID.
    /// </summary>
    public async Task<List<Item>> GetAllByShoppingListIdAsync(Guid shoppingListId, CancellationToken ct = default)
    {
        return await _appDbContext.Items.Where(item => item.ShoppingListId == shoppingListId).ToListAsync(ct);
    }


    /// <summary>
    /// Creates a new item in the specified shopping list and returns its ID.
    /// Does not save changes to the database.
    /// </summary>
    public async Task<Guid> CreateAsync(Guid shoppingListId, ItemPostDto itemPostDto, CancellationToken ct = default)
    {
        var newItemId = Guid.NewGuid();

        var newItem = new Item
        {
            ItemId = newItemId,
            ShoppingListId = shoppingListId,
            ItemName = itemPostDto.ItemName,
            ItemAmount = itemPostDto.ItemAmount
        };

        await _appDbContext.Items.AddAsync(newItem, ct);

        return newItemId;
    }

    /// <summary>
    /// Updates the specified item with new values from the patch DTO.
    /// </summary>
    public void UpdateById(Item targetItem, ItemPatchDto itemPatchDto)
    {
        var (itemName, itemAmount) = itemPatchDto;

        if (itemName is not null) targetItem.ItemName = itemName;
        if (itemAmount is not null) targetItem.ItemAmount = itemAmount;
    }

    /// <summary>
    /// Removes the specified item from the database context. Does not save changes.
    /// </summary>
    public void Delete(Item targetItem)
    {
        _appDbContext.Items.Remove(targetItem);
    }

    /// <summary>
    /// Removes a batch of items from the database context. Does not save changes.
    /// </summary>
    public void DeleteBatch(List<Item> items)
    {
        _appDbContext.Items.RemoveRange(items);
    }
}