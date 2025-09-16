using Microsoft.EntityFrameworkCore;
using ShoppingListApi.Data.Contexts;
using ShoppingListApi.Enums;
using ShoppingListApi.Interfaces.Repositories;
using ShoppingListApi.Model.DTOs.Patch;
using ShoppingListApi.Model.DTOs.Post;
using ShoppingListApi.Model.Entity;
using ShoppingListApi.Model.ReturnTypes;

namespace ShoppingListApi.Repositories;

public class ListUserRepository(AppDbContext appAppDbContext, ILogger<ListUserRepository> logger) : IListUserRepository
{
    private readonly ILogger<ListUserRepository> _logger = logger;
    private readonly AppDbContext _appDbContext = appAppDbContext;


    public async Task<ListUser?> GetWithoutDetailsByIdAsync(Guid listUserId, CancellationToken ct = default)
    {
        return await _appDbContext.ListUsers.FindAsync([listUserId], ct);
    }

    public async Task<ListUser?> GetWithoutDetailsByEmailAddressAsync(string emailAddress,
        CancellationToken ct = default)
    {
        return await _appDbContext.ListUsers.FirstOrDefaultAsync(lu => lu.EmailAddress == emailAddress, ct);
    }

    public async Task<Guid?> CreateAsync(ListUserPostExtendedDto listUserPostExtendedDto, CancellationToken ct)
    {
        var newUserId = Guid.NewGuid();

        var newUser = new ListUser()
        {
            UserId = newUserId,
            FirstName = listUserPostExtendedDto.FirstName,
            LastName = listUserPostExtendedDto.LastName,
            EmailAddress = listUserPostExtendedDto.EmailAddress,
            PasswordHash = listUserPostExtendedDto.PasswordHash,
            CreationDateTime = listUserPostExtendedDto.CreationDateTime
        };

        await _appDbContext.ListUsers.AddAsync(newUser, ct);

        var checkResult = await _appDbContext.SaveChangesAsync(ct);

        if (checkResult != 1)
            return null;

        return newUserId;
    }

    public async Task<bool> UpdateNameAsync(ListUser listUser, ListUserPatchDto listUserPatchDto,
        CancellationToken ct = default)
    {
        if (listUserPatchDto.FirstName is null && listUserPatchDto.LastName is null)
            return false;

        if (listUserPatchDto.FirstName is not null)
            listUser.FirstName = listUserPatchDto.FirstName;

        if (listUserPatchDto.LastName is not null)
            listUser.LastName = listUserPatchDto.LastName;

        var checkResult = await _appDbContext.SaveChangesAsync(ct);

        if (checkResult != 1)
            return false;

        return true;
    }

    public async Task<bool> UpdatePassword(ListUser listUser, string newPasswordHash, CancellationToken ct = default)
    {
        listUser.PasswordHash = newPasswordHash;

        var checkResult = await _appDbContext.SaveChangesAsync(ct);

        if (checkResult != 1)
            return false;

        return true;
    }

    public async Task<RemoveRecordResult> DeleteAsync(ListUser listUser, CancellationToken ct = default)
    {
        await using var transaction = await _appDbContext.Database.BeginTransactionAsync(ct);
        int deletedRecords = 0;

        try
        {
            // remove list memberships

            var listMemberships = await _appDbContext.ListMemberships
                .Include(lm => lm.UserRole)
                .Where(lm => lm.UserId == listUser.UserId).ToListAsync(ct);

            if (listMemberships.Any())
            {
                var ownerLists = listMemberships.Where(lm => lm.UserRole!.EnumIndex == (int)UserRoleEnum.ListOwner)
                    .ToList();
                var ownerShoppingListIds = ownerLists.Select(lm => lm.ShoppingListId);

                if (ownerLists.Count > 0)
                {
                    // delete items in owner lists
                    var checkItemsDeleted = await _appDbContext.Items
                        .Where(item => ownerShoppingListIds.Contains(item.ShoppingListId))
                        .ExecuteDeleteAsync(ct);

                    // delete owner lists
                    var checkListsDeleted = await _appDbContext.ShoppingLists
                        .Where(sl => ownerShoppingListIds.Contains(sl.ShoppingListId))
                        .ExecuteDeleteAsync(ct);

                    if (checkItemsDeleted < 0 || checkListsDeleted < 0 || checkListsDeleted != ownerLists.Count)
                    {
                        await transaction.RollbackAsync(ct);
                        return new RemoveRecordResult(true, false, 0);
                    }

                    deletedRecords += checkItemsDeleted + checkListsDeleted;
                }

                // delete all memberships

                var checkMembershipsDelete = await _appDbContext.ListMemberships
                    .Where(lm => lm.UserId == listUser.UserId).ExecuteDeleteAsync(ct);

                if (checkMembershipsDelete != listMemberships.Count)
                {
                    await transaction.RollbackAsync(ct);
                    return new RemoveRecordResult(true, false, 0);
                }

                deletedRecords += checkMembershipsDelete;
            }

            // remove email confirmationTokens
            var checkEmailTokensDeleted = await _appDbContext.EmailConfirmationTokens
                .Where(ect => ect.UserId == listUser.UserId)
                .ExecuteDeleteAsync(ct);

            if (checkEmailTokensDeleted < 0)
            {
                await transaction.RollbackAsync(ct);
                return new RemoveRecordResult(true, false, 0);
            }

            deletedRecords += checkEmailTokensDeleted;

            // remove api keys
            var checkApiKeysDeleted = await _appDbContext.ApiKeys
                .Where(ak => ak.UserId == listUser.UserId)
                .ExecuteDeleteAsync(ct);

            if (checkApiKeysDeleted < 0)
            {
                await transaction.RollbackAsync(ct);
                return new RemoveRecordResult(true, false, 0);
            }

            deletedRecords += checkApiKeysDeleted;

            // remove user

            _appDbContext.ListUsers.Remove(listUser);
            var checkUserDeleted = await _appDbContext.SaveChangesAsync(ct);

            if (checkUserDeleted != 1)
            {
                await transaction.RollbackAsync(ct);
                return new RemoveRecordResult(true, false, 0);
            }

            deletedRecords += checkUserDeleted;

            await transaction.CommitAsync(ct);
            return new RemoveRecordResult(true, true, deletedRecords);
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync(ct);
            _logger.LogError("Error deleting user {UserId}: {ExceptionMessage}", listUser.UserId, e.Message);
            throw;
        }
    }
}