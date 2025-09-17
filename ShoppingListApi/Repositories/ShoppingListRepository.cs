using Microsoft.EntityFrameworkCore;
using ShoppingListApi.Data.Contexts;
using ShoppingListApi.Interfaces.Repositories;
using ShoppingListApi.Interfaces.Services;
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

    public async Task<Guid?> CreateAsync(ShoppingListPostDto shoppingListPostDto, CancellationToken ct = default)
    {
        var newShoppingListId = Guid.NewGuid();

        var newShoppingList = new ShoppingList()
        {
            ShoppingListId = newShoppingListId,
            ShoppingListName = shoppingListPostDto.ShoppingListName
        };

        await _appDbContext.ShoppingLists.AddAsync(newShoppingList, ct);

        var checkResult = await _appDbContext.SaveChangesAsync(ct);

        if (checkResult != 1)
            return null;

        return newShoppingListId;
    }

    public async Task<Guid?> CreateAndAssignInTransactionAsync(IListMembershipService listMembershipService,
        Guid userId, ShoppingListPostDto shoppingListPostDto, Guid ownerUserRoleId, CancellationToken ct = default)
    {
        await using var transaction = await _appDbContext.Database.BeginTransactionAsync(ct);

        try
        {
            var newShoppingListId = Guid.NewGuid();

            var newShoppingList = new ShoppingList()
            {
                ShoppingListId = newShoppingListId,
                ShoppingListName = shoppingListPostDto.ShoppingListName
            };

            await _appDbContext.ShoppingLists.AddAsync(newShoppingList, ct);

            var checkResult = await _appDbContext.SaveChangesAsync(ct);

            if (checkResult != 1)
            {
                await transaction.RollbackAsync(ct);
                return null;
            }

            var listOwnerAdditionResult =
                await listMembershipService.AssignUserToShoppingListAsync(userId, newShoppingListId, ownerUserRoleId,
                    ct);

            if (listOwnerAdditionResult.Success is false)
            {
                _logger.LogError(
                    "Failed to assign user with ID {UserId} as owner to newly created shopping list with ID {ShoppingListId}. Shopping list creation aborted!",
                    userId, newShoppingListId);
                await transaction.RollbackAsync(ct);
                return null;
            }

            await transaction.CommitAsync(ct);
            return newShoppingListId;
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync(ct);
            _logger.LogError(
                "Error creating shopping list and assigning owner with ID {UserId}. The following exception was caught: {Exception}",
                userId, e);
            throw;
        }
    }

    public async Task<bool> UpdateNameAsync(ShoppingList targetShoppingList, ShoppingListPostDto shoppingListPostDto,
        CancellationToken ct = default)
    {
        targetShoppingList.ShoppingListName = shoppingListPostDto.ShoppingListName;

        var checkResult = await _appDbContext.SaveChangesAsync(ct);

        return checkResult == 1;
    }

    public async Task<RemoveRecordResult> DeleteAndCascadeAsync(ShoppingList targetShoppingList,
        CancellationToken ct = default)
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

    public async Task<bool> DeleteAndCascadeByIdAsync(Guid targetShoppingListId,
        CancellationToken ct = default)
    {
        await using var transaction = await _appDbContext.Database.BeginTransactionAsync(ct);

        int recordsToBeRemoved = 0;

        try
        {
            // delete items
            var shoppingListItems = await _appDbContext.Items
                .Where(i => i.ShoppingListId == targetShoppingListId).ToListAsync(ct);

            recordsToBeRemoved += shoppingListItems.Count;

            _appDbContext.Items.RemoveRange(shoppingListItems);

            // delete memberships
            var shoppingListMemberships = await _appDbContext.ListMemberships
                .Where(lm => lm.ShoppingListId == targetShoppingListId).ToListAsync(ct);

            recordsToBeRemoved += shoppingListMemberships.Count;

            _appDbContext.ListMemberships.RemoveRange(shoppingListMemberships);

            var checkResult = await _appDbContext.SaveChangesAsync(ct);

            // delete shopping list
            recordsToBeRemoved += 1;

            var targetDeletionResult = await _appDbContext.ShoppingLists
                .Where(sl => sl.ShoppingListId == targetShoppingListId).ExecuteDeleteAsync(ct);

            checkResult += targetDeletionResult;

            if (checkResult != recordsToBeRemoved)
            {
                await transaction.RollbackAsync(ct);
                return false;
            }

            await transaction.CommitAsync(ct);
            return true;
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync(ct);
            _logger.LogError(e, "Error deleting shopping list with ID {ShoppingListId}",
                targetShoppingListId);
            Console.WriteLine(e);
            throw;
        }
    }
}