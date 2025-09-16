using Microsoft.EntityFrameworkCore;
using ShoppingListApi.Data.Contexts;
using ShoppingListApi.Enums;
using ShoppingListApi.Interfaces.Repositories;
using ShoppingListApi.Model.Entity;

namespace ShoppingListApi.Repositories;

public class ListMembershipRepository(AppDbContext appDbContext, ILogger<ListMembershipRepository> logger)
    : IListMembershipRepository
{
    private readonly AppDbContext _appDbContext = appDbContext;
    private readonly ILogger<ListMembershipRepository> _logger = logger;

    public async Task<UserRole?> GetUserRoleInShoppingListAsync(Guid listUserId, Guid shoppingListId,
        CancellationToken ct = default)
    {
        var targetMembership = await _appDbContext.ListMemberships
            .Include(lm => lm.UserRole)
            .FirstOrDefaultAsync(lm => lm.UserId == listUserId && lm.ShoppingListId == shoppingListId, ct);

        if (targetMembership is null) return null;

        return targetMembership.UserRole;
    }

    public async Task<ListMembership?> GetListMembershipByCompositePkAsync(Guid listUserId, Guid shoppingListId,
        CancellationToken ct = default)
    {
        return await _appDbContext.ListMemberships.FindAsync([shoppingListId, listUserId], ct);
    }

    public async Task<List<Guid>> GetAllShoppingListIdsForUserAsync(Guid listUserId,
        CancellationToken ct = default)
    {
        return (await _appDbContext.ListMemberships
                .Where(lm => lm.UserId == listUserId).ToListAsync(ct))
            .Select(lm => lm.ShoppingListId).ToList();
    }

    public async Task<List<ShoppingList>> GetAllShoppingListsOwnedByUserAsync(Guid listUserId,
        CancellationToken ct = default)
    {
        return (await _appDbContext.ListMemberships
                .Include(lm => lm.ShoppingList)
                .Include(lm => lm.UserRole)
                .Where(lm => lm.UserId == listUserId && lm.UserRole!.EnumIndex == (int)UserRoleEnum.ListOwner)
                .ToListAsync(ct))
            .Select(lm => lm.ShoppingList!).ToList();
    }

    public async Task<List<ShoppingList>> GetAllCollaboratingShoppingListsForUserAsync(Guid listUserId,
        CancellationToken ct = default)
    {
        return (await _appDbContext.ListMemberships
                .Include(lm => lm.ShoppingList)
                .Include(lm => lm.UserRole)
                .Where(lm => lm.UserId == listUserId && lm.UserRole!.EnumIndex == (int)UserRoleEnum.Collaborator)
                .ToListAsync(ct))
            .Select(lm => lm.ShoppingList!).ToList();
    }

    public async Task<List<ListMembership>> GetAllUsersInShoppingListAsync(Guid shoppingListId,
        CancellationToken ct = default)
    {
        return await _appDbContext.ListMemberships
            .Include(lm => lm.UserRole)
            .Include(lm => lm.User)
            .Where(lm => lm.ShoppingListId == shoppingListId)
            .ToListAsync(ct);
    }

    public async Task<ListUser?> GetShoppingListOwner(Guid shoppingListId, CancellationToken ct = default)
    {
        var potentialOwnerList = await _appDbContext.ListMemberships
            .Include(lm => lm.UserRole)
            .Include(lm => lm.User)
            .Where(lm => lm.ShoppingListId == shoppingListId && lm.UserRole!.EnumIndex == (int)UserRoleEnum.ListOwner)
            .ToListAsync(ct);

        if (potentialOwnerList.Count > 1)
            throw new Exception("Data integrity issue: multiple list owners found for shopping list ID " +
                                shoppingListId);

        return potentialOwnerList.FirstOrDefault()?.User;
    }

    public async Task<List<ListUser>> GetShoppingListCollaborators(Guid shoppingListId,
        CancellationToken ct = default)
    {
        var collaboratorsList = await _appDbContext.ListMemberships
            .Include(lm => lm.UserRole)
            .Include(lm => lm.User)
            .Where(lm =>
                lm.ShoppingListId == shoppingListId && lm.UserRole!.EnumIndex == (int)UserRoleEnum.Collaborator)
            .ToListAsync(ct);

        return collaboratorsList.Select(lm => lm.User!).ToList();
    }

    public async Task<bool> AssignUserToShoppingListByUserRoleIdAsync(Guid listUserId, Guid shoppingListId,
        Guid userRoleId,
        CancellationToken ct = default)
    {
        var newMembership = new ListMembership()
        {
            UserId = listUserId,
            ShoppingListId = shoppingListId,
            UserRoleId = userRoleId
        };

        await _appDbContext.ListMemberships.AddAsync(newMembership, ct);

        var checkResult = await _appDbContext.SaveChangesAsync(ct);

        return checkResult == 1;
    }

    public async Task<bool> RemoveListMembershipAsync(ListMembership listMembership,
        CancellationToken ct = default)
    {
        _appDbContext.ListMemberships.Remove(listMembership);

        var checkResult = await _appDbContext.SaveChangesAsync(ct);

        return checkResult == 1;
    }
}