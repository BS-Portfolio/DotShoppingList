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


    public async Task<Item?> GetByIdAsync(Guid shoppingListId, Guid itemId, CancellationToken ct = default)
    {
        return await _appDbContext.Items.FirstOrDefaultAsync(item =>
            item.ShoppingListId == shoppingListId && item.ItemId == itemId, ct);
    }

    public async Task<List<Item>> GetAllByShoppingListIdAsync(Guid shoppingListId, CancellationToken ct = default)
    {
        return await _appDbContext.Items.Where(item => item.ShoppingListId == shoppingListId).ToListAsync(ct);
    }


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

    public void UpdateById(Item targetItem, ItemPatchDto itemPatchDto)
    {
        var (itemName, itemAmount) = itemPatchDto;

        if (itemName is not null) targetItem.ItemName = itemName;
        if (itemAmount is not null) targetItem.ItemAmount = itemAmount;
    }

    public void Delete(Item targetItem)
    {
        _appDbContext.Items.Remove(targetItem);
    }

    public void DeleteBatch(List<Item> items)
    {
        _appDbContext.Items.RemoveRange(items);
    }
}