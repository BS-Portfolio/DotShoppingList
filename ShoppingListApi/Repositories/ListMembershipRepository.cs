using Microsoft.EntityFrameworkCore;
using ShoppingListApi.Data.Contexts;
using ShoppingListApi.Enums;
using ShoppingListApi.Interfaces.Repositories;
using ShoppingListApi.Model.Entity;

namespace ShoppingListApi.Repositories;

public class ListMembershipRepository(AppDbContext appDbContext)
    : IListMembershipRepository
{
    private readonly AppDbContext _appDbContext = appDbContext;

    public async Task<UserRole?> GetUserRoleObjInShoppingListAsync(Guid listUserId, Guid shoppingListId,
        CancellationToken ct = default)
    {
        var targetMembership = await _appDbContext.ListMemberships
            .Include(lm => lm.UserRole)
            .FirstOrDefaultAsync(lm => lm.UserId == listUserId && lm.ShoppingListId == shoppingListId, ct);

        if (targetMembership is null) return null;

        return targetMembership.UserRole;
    }

    public async Task<UserRoleEnum?> GetUserRoleEnumInShoppingListAsync(Guid listUserId, Guid shoppingListId,
        CancellationToken ct = default)
    {
        var targetMembership = await _appDbContext.ListMemberships
            .Include(lm => lm.UserRole)
            .FirstOrDefaultAsync(lm => lm.UserId == listUserId && lm.ShoppingListId == shoppingListId, ct);

        if (targetMembership?.UserRole is null) return null;

        var targetEnum = UserRole.GetEnumFromIndex(targetMembership.UserRole.EnumIndex);

        return targetEnum;
    }

    public async Task<ListMembership?> GetListMembershipByCompositePkAsync(Guid shoppingListId, Guid listUserId,
        CancellationToken ct = default)
    {
        return await _appDbContext.ListMemberships.FindAsync([shoppingListId, listUserId], ct);
    }

    public async Task<List<Guid>> GetAllShoppingListIdsForUserAsync(Guid listUserId,
        CancellationToken ct = default)
    {
        return await _appDbContext.ListMemberships
            .Where(lm => lm.UserId == listUserId)
            .Select(lm => lm.ShoppingListId)
            .ToListAsync(ct);
    }

    public async Task<List<ListMembership>> GetAllListMembershipsWithDetailsForOwnerByUserAsync(Guid listUserId,
        CancellationToken ct = default)
    {
        return await _appDbContext.ListMemberships
            .Include(lm => lm.ShoppingList)
            .Include(lm => lm.UserRole)
            .Where(lm => lm.UserId == listUserId && lm.UserRole!.EnumIndex == (int)UserRoleEnum.ListOwner)
            .ToListAsync(ct);
    }

    public async Task<List<ListMembership>> GetAllListMembershipsWithDetailsForNotOwnerByUserAsync(Guid listUserId,
        CancellationToken ct = default)
    {
        return await _appDbContext.ListMemberships
            .Include(lm => lm.ShoppingList)
            .Include(lm => lm.UserRole)
            .Where(lm => lm.UserId == listUserId && lm.UserRole!.EnumIndex != (int)UserRoleEnum.ListOwner)
            .ToListAsync(ct);
    }

    public async Task<List<ListMembership>> GetAllMembershipsWithoutCascadingInfoByShoppingListIdAsync(
        Guid shoppingListId,
        CancellationToken ct = default)
    {
        return await _appDbContext.ListMemberships
            .Where(lm => lm.ShoppingListId == shoppingListId)
            .ToListAsync(ct);
    }

    public async Task<List<ListMembership>> GetAllMembershipsWithCascadingInfoByShoppingListIdAsync(Guid shoppingListId,
        CancellationToken ct = default)
    {
        return await _appDbContext.ListMemberships
            .Include(lm => lm.User)
            .Include(lm => lm.UserRole)
            .Where(lm => lm.ShoppingListId == shoppingListId)
            .ToListAsync(ct);
    }

    public async Task<List<ListMembership>> GetAllMembershipsWithCascadingInfoByShoppingListIdsAsync(
        List<Guid> shoppingListIds,
        CancellationToken ct = default)
    {
        return await _appDbContext.ListMemberships
            .Include(lm => lm.User)
            .Include(lm => lm.UserRole)
            .Where(lm => shoppingListIds.Contains(lm.ShoppingListId))
            .ToListAsync(ct);
    }

    public async Task<List<ListMembership>> GetAllMembershipsWithCascadingInfoByUserIdAsync(Guid listUserId,
        CancellationToken ct = default)
    {
        return await _appDbContext.ListMemberships
            .Include(lm => lm.ShoppingList).ThenInclude(sl => sl!.Items)
            .Include(lm => lm.UserRole)
            .Include(lm => lm.User)
            .Where(lm => lm.UserId == listUserId)
            .ToListAsync(ct);
    }

    public async Task<List<ShoppingList>> GetAllCollaboratingShoppingListsForUserAsync(Guid listUserId,
        CancellationToken ct = default)
    {
        return await _appDbContext.ListMemberships
            .Include(lm => lm.ShoppingList)
            .Include(lm => lm.UserRole)
            .Where(lm => lm.UserId == listUserId && lm.UserRole!.EnumIndex == (int)UserRoleEnum.Collaborator)
            .Select(lm => lm.ShoppingList!)
            .ToListAsync(ct);
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
        var ownerMembership = await _appDbContext.ListMemberships
            .Include(lm => lm.UserRole)
            .Include(lm => lm.User)
            .Where(lm => lm.ShoppingListId == shoppingListId && lm.UserRole!.EnumIndex == (int)UserRoleEnum.ListOwner)
            .FirstOrDefaultAsync(ct);

        if (ownerMembership == null)
            return null;

        // Check for data integrity
        var ownerCount = await _appDbContext.ListMemberships
            .CountAsync(
                lm => lm.ShoppingListId == shoppingListId && lm.UserRole!.EnumIndex == (int)UserRoleEnum.ListOwner, ct);

        if (ownerCount > 1)
            throw new Exception(
                $"Data integrity issue: multiple list owners found for shopping list ID {shoppingListId}");

        return ownerMembership.User;
    }

    public async Task<List<ListUser>> GetShoppingListCollaborators(Guid shoppingListId,
        CancellationToken ct = default)
    {
        return await _appDbContext.ListMemberships
            .Include(lm => lm.UserRole)
            .Include(lm => lm.User)
            .Where(lm =>
                lm.ShoppingListId == shoppingListId && lm.UserRole!.EnumIndex == (int)UserRoleEnum.Collaborator)
            .Select(lm => lm.User!)
            .ToListAsync(ct);
    }

    public Task<ShoppingList?> OwnsShoppingListWithNameAsync(Guid userId, string listName, Guid? excludeListId,
        CancellationToken ct = default)
    {
        var query = _appDbContext.ListMemberships
            .Include(lm => lm.ShoppingList)
            .Include(lm => lm.UserRole)
            .Where(lm => lm.UserId == userId && lm.UserRole!.EnumIndex == (int)UserRoleEnum.ListOwner &&
                         lm.ShoppingList!.ShoppingListName == listName)
            .Select(lm => lm.ShoppingList);

        if (excludeListId.HasValue)
            query = query.Where(q => q!.ShoppingListId != excludeListId.Value);

        return query.FirstOrDefaultAsync(ct);
    }

    public async Task<ListMembership> AssignUserToShoppingListByUserRoleIdAsync(Guid listUserId, Guid shoppingListId,
        Guid userRoleId, CancellationToken ct = default)
    {
        var newMembership = new ListMembership()
        {
            UserId = listUserId,
            ShoppingListId = shoppingListId,
            UserRoleId = userRoleId
        };

        await _appDbContext.ListMemberships.AddAsync(newMembership, ct);

        return newMembership;
    }

    public void Delete(ListMembership listMembership)
    {
        _appDbContext.ListMemberships.Remove(listMembership);
    }

    public void DeleteBatch(List<ListMembership> listMembership)
    {
        _appDbContext.ListMemberships.RemoveRange(listMembership);
    }
}