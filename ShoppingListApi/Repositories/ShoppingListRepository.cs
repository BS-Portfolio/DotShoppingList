using Microsoft.EntityFrameworkCore;
using ShoppingListApi.Data.Contexts;
using ShoppingListApi.Interfaces.Repositories;
using ShoppingListApi.Model.DTOs.Post;
using ShoppingListApi.Model.Entity;
using ShoppingListApi.Model.ReturnTypes;

namespace ShoppingListApi.Repositories;

public class ShoppingListRepository(AppDbContext appDbContext, ILogger<ShoppingListRepository> logger)
    : IShoppingListRepository
{
    private readonly AppDbContext _appDbContext = appDbContext;

    private readonly ILogger<ShoppingListRepository> _logger = logger;


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

    public async Task<Guid?> CreateAsync(ShoppingListPost shoppingListPost, CancellationToken ct = default)
    {
        var newShoppingListId = Guid.NewGuid();

        var newShoppingList = new ShoppingList()
        {
            ShoppingListId = newShoppingListId,
            ShoppingListName = shoppingListPost.ShoppingListName,
        };

        await _appDbContext.ShoppingLists.AddAsync(newShoppingList, ct);

        var checkResult = await _appDbContext.SaveChangesAsync(ct);

        if (checkResult != 1)
            return null;

        return newShoppingListId;
    }

    public async Task<bool> UpdateNameAsync(ShoppingList targetShoppingList, ShoppingListPost shoppingListPost,
        CancellationToken ct = default)
    {
        targetShoppingList.ShoppingListName = shoppingListPost.ShoppingListName;

        var checkResult = await _appDbContext.SaveChangesAsync(ct);

        return checkResult == 1;
    }

    public async Task<RemoveRecordResult> DeleteAndCascadeAsync(ShoppingList targetShoppingList, CancellationToken ct = default)
    {
        await using var transaction = await _appDbContext.Database.BeginTransactionAsync(ct);

        int recordsToBeRemoved = 0;

        try
        {
            // delete items
            var shoppingListItems = await _appDbContext.Items
                .Where(i => i.ShoppingListId == targetShoppingList.ShoppingListId).ToListAsync(ct);

            recordsToBeRemoved += shoppingListItems.Count;

            _appDbContext.Items.RemoveRange(shoppingListItems);

            // delete memberships
            var shoppingListMemberships = await _appDbContext.ListMemberships
                .Where(lm => lm.ShoppingListId == targetShoppingList.ShoppingListId).ToListAsync(ct);

            recordsToBeRemoved += shoppingListMemberships.Count;

            _appDbContext.ListMemberships.RemoveRange(shoppingListMemberships);

            // delete shopping list
            recordsToBeRemoved += 1;
            _appDbContext.ShoppingLists.Remove(targetShoppingList);

            var checkResult = await _appDbContext.SaveChangesAsync(ct);

            if (checkResult != recordsToBeRemoved)
            {
                await transaction.RollbackAsync(ct);
                return new(true, false, 0);
            }

            await transaction.CommitAsync(ct);
            return new(true, true, checkResult);
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync(ct);
            _logger.LogError(e, "Error deleting shopping list with ID {ShoppingListId}",
                targetShoppingList.ShoppingListId);
            Console.WriteLine(e);
            throw;
        }
    }
}