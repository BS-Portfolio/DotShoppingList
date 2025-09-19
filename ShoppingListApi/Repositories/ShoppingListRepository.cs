using Microsoft.EntityFrameworkCore;
using ShoppingListApi.Data.Contexts;
using ShoppingListApi.Interfaces.Repositories;
using ShoppingListApi.Model.DTOs.Post;
using ShoppingListApi.Model.Entity;

namespace ShoppingListApi.Repositories;

public class ShoppingListRepository(AppDbContext appDbContext)
    : IShoppingListRepository
{
    private readonly AppDbContext _appDbContext = appDbContext;

    public async Task<ShoppingList?> GetWithoutItemsByIdAsync(Guid shoppingListId, CancellationToken ct = default)
    {
        return await _appDbContext.ShoppingLists.FindAsync([shoppingListId], ct);
    }

    public async Task<ShoppingList?> GetWithItemsByIdAsync(Guid shoppingListId, CancellationToken ct = default)
    {
        return await _appDbContext.ShoppingLists
            .Include(sl => sl.Items)
            .FirstOrDefaultAsync(sl => sl.ShoppingListId == shoppingListId, ct);
    }

    public async Task<Guid> CreateAsync(ShoppingListPostDto shoppingListPostDto, CancellationToken ct = default)
    {
        var newShoppingListId = Guid.NewGuid();

        var newShoppingList = new ShoppingList()
        {
            ShoppingListId = newShoppingListId,
            ShoppingListName = shoppingListPostDto.ShoppingListName
        };

        await _appDbContext.ShoppingLists.AddAsync(newShoppingList, ct);

        return newShoppingListId;
    }

    public void UpdateName(ShoppingList targetShoppingList, ShoppingListPostDto shoppingListPostDto)
    {
        targetShoppingList.ShoppingListName = shoppingListPostDto.ShoppingListName;
    }

    public void Delete(ShoppingList shoppingList)
    {
        _appDbContext.ShoppingLists.Remove(shoppingList);
    }

    public void DeleteBatch(List<ShoppingList> shoppingLists)
    {
        _appDbContext.RemoveRange(shoppingLists);
    }
}