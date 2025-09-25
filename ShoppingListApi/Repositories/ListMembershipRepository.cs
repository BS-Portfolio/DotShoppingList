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

    /// <summary>
    /// Retrieves the UserRole object for a user in a specific shopping list.
    /// Returns null if no membership is found.
    /// </summary>
    public async Task<UserRole?> GetUserRoleObjInShoppingListAsync(Guid listUserId, Guid shoppingListId,
        CancellationToken ct = default)
    {
        var targetMembership = await _appDbContext.ListMemberships
            .Include(lm => lm.UserRole)
            .FirstOrDefaultAsync(lm => lm.UserId == listUserId && lm.ShoppingListId == shoppingListId, ct);

        if (targetMembership is null) return null;

        return targetMembership.UserRole;
    }

    /// <summary>
    /// Retrieves the UserRoleEnum for a user in a specific shopping list.
    /// Returns null if no membership or role is found.
    /// </summary>
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

    /// <summary>
    /// Retrieves a ListMembership by its composite primary key (shoppingListId, listUserId).
    /// Returns null if not found.
    /// </summary>
    public async Task<ListMembership?> GetListMembershipByCompositePkAsync(Guid shoppingListId, Guid listUserId,
        CancellationToken ct = default)
    {
        return await _appDbContext.ListMemberships.FindAsync([shoppingListId, listUserId], ct);
    }

    /// <summary>
    /// Retrieves all shopping list IDs for a given user.
    /// </summary>
    public async Task<List<Guid>> GetAllShoppingListIdsForUserAsync(Guid listUserId,
        CancellationToken ct = default)
    {
        return await _appDbContext.ListMemberships
            .Where(lm => lm.UserId == listUserId)
            .Select(lm => lm.ShoppingListId)
            .ToListAsync(ct);
    }

    /// <summary>
    /// Retrieves all ListMemberships for a user where the user is the list owner, including ShoppingList and UserRole details.
    /// </summary>
    public async Task<List<ListMembership>> GetAllListMembershipsWithDetailsForOwnerByUserAsync(Guid listUserId,
        CancellationToken ct = default)
    {
        return await _appDbContext.ListMemberships
            .Include(lm => lm.ShoppingList)
            .Include(lm => lm.UserRole)
            .Where(lm => lm.UserId == listUserId && lm.UserRole!.EnumIndex == (int)UserRoleEnum.ListOwner)
            .ToListAsync(ct);
    }

    /// <summary>
    /// Retrieves all ListMemberships for a user where the user is not the list owner, including ShoppingList and UserRole details.
    /// </summary>
    public async Task<List<ListMembership>> GetAllListMembershipsWithDetailsForNotOwnerByUserAsync(Guid listUserId,
        CancellationToken ct = default)
    {
        return await _appDbContext.ListMemberships
            .Include(lm => lm.ShoppingList)
            .Include(lm => lm.UserRole)
            .Where(lm => lm.UserId == listUserId && lm.UserRole!.EnumIndex != (int)UserRoleEnum.ListOwner)
            .ToListAsync(ct);
    }

    /// <summary>
    /// Retrieves all ListMemberships for a shopping list without including UserRole, ShoppingList, or ListUser details.
    /// </summary>
    public async Task<List<ListMembership>> GetAllMembershipsWithoutCascadingInfoByShoppingListIdAsync(
        Guid shoppingListId,
        CancellationToken ct = default)
    {
        return await _appDbContext.ListMemberships
            .Where(lm => lm.ShoppingListId == shoppingListId)
            .ToListAsync(ct);
    }

    /// <summary>
    /// Retrieves all ListMemberships for a shopping list, including User and UserRole details.
    /// </summary>
    public async Task<List<ListMembership>> GetAllMembershipsWithCascadingInfoByShoppingListIdAsync(Guid shoppingListId,
        CancellationToken ct = default)
    {
        return await _appDbContext.ListMemberships
            .Include(lm => lm.User)
            .Include(lm => lm.UserRole)
            .Where(lm => lm.ShoppingListId == shoppingListId)
            .ToListAsync(ct);
    }

    /// <summary>
    /// Retrieves all ListMemberships for multiple shopping lists, including User and UserRole details.
    /// </summary>
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

    /// <summary>
    /// Retrieves all ListMemberships for a user, including ShoppingList (with Items), UserRole, and User details.
    /// </summary>
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

    /// <summary>
    /// Retrieves all shopping lists where the user is a collaborator.
    /// </summary>
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

    /// <summary>
    /// Retrieves all users in a shopping list, including UserRole and User details.
    /// </summary>
    public async Task<List<ListMembership>> GetAllUsersInShoppingListAsync(Guid shoppingListId,
        CancellationToken ct = default)
    {
        return await _appDbContext.ListMemberships
            .Include(lm => lm.UserRole)
            .Include(lm => lm.User)
            .Where(lm => lm.ShoppingListId == shoppingListId)
            .ToListAsync(ct);
    }

    /// <summary>
    /// Retrieves the owner (ListUser) of a shopping list. Throws an exception if multiple owners are found.
    /// Returns null if no owner is found.
    /// </summary>
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

    /// <summary>
    /// Retrieves all collaborators (ListUser) in a shopping list.
    /// </summary>
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

    /// <summary>
    /// Checks if a user owns a shopping list with a given name, optionally excluding a specific list ID.
    /// Returns the ShoppingList if found, otherwise null.
    /// </summary>
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

    /// <summary>
    /// Assigns a user to a shopping list with a specific user role ID. Does not save changes to the database.
    /// Returns the new ListMembership.
    /// </summary>
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

    /// <summary>
    /// Removes the specified ListMembership from the database context. Does not save changes.
    /// </summary>
    public void Delete(ListMembership listMembership)
    {
        _appDbContext.ListMemberships.Remove(listMembership);
    }

    /// <summary>
    /// Removes a batch of ListMemberships from the database context. Does not save changes.
    /// </summary>
    public void DeleteBatch(List<ListMembership> listMembership)
    {
        _appDbContext.ListMemberships.RemoveRange(listMembership);
    }
}