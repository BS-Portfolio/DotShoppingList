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

    /// <summary>
    /// Retrieves a ShoppingList by its ID without including related items.
    /// Returns null if not found.
    /// </summary>
    public async Task<ShoppingList?> GetWithoutItemsByIdAsync(Guid shoppingListId, CancellationToken ct = default)
    {
        return await _appDbContext.ShoppingLists.FindAsync([shoppingListId], ct);
    }

    /// <summary>
    /// Retrieves a ShoppingList by its ID, including related items.
    /// Returns null if not found.
    /// </summary>
    public async Task<ShoppingList?> GetWithItemsByIdAsync(Guid shoppingListId, CancellationToken ct = default)
    {
        return await _appDbContext.ShoppingLists
            .Include(sl => sl.Items)
            .FirstOrDefaultAsync(sl => sl.ShoppingListId == shoppingListId, ct);
    }

    /// <summary>
    /// Creates a new ShoppingList from the provided DTO and returns its ID.
    /// Does not save changes to the database.
    /// </summary>
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

    /// <summary>
    /// Updates the name of the specified ShoppingList using the provided DTO.
    /// </summary>
    public void UpdateName(ShoppingList targetShoppingList, ShoppingListPostDto shoppingListPostDto)
    {
        targetShoppingList.ShoppingListName = shoppingListPostDto.ShoppingListName;
    }

    /// <summary>
    /// Removes the specified ShoppingList from the database context. Does not save changes.
    /// </summary>
    public void Delete(ShoppingList shoppingList)
    {
        _appDbContext.ShoppingLists.Remove(shoppingList);
    }

    /// <summary>
    /// Removes a batch of ShoppingLists from the database context. Does not save changes.
    /// </summary>
    public void DeleteBatch(List<ShoppingList> shoppingLists)
    {
        _appDbContext.RemoveRange(shoppingLists);
    }
}