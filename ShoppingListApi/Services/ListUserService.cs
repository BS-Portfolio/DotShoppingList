using ShoppingListApi.Configs;
using ShoppingListApi.Enums;
using ShoppingListApi.Exceptions;
using ShoppingListApi.Interfaces.Services;
using ShoppingListApi.Model.DTOs.Create;
using ShoppingListApi.Model.DTOs.Patch;
using ShoppingListApi.Model.Entity;
using ShoppingListApi.Model.ReturnTypes;

namespace ShoppingListApi.Services;

public class ListUserService(IUnitOfWork unitOfWork, ILogger<ListUserService> logger) : IListUserService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    private readonly ILogger<ListUserService> _logger = logger;

    public async Task<AddRecordResult<Guid?, ListUser?>> CheckConflictAndCreateUserAsync(
        ListUserCreateDto listUserCreateDto,
        CancellationToken ct = default)
    {
        try
        {
            var conflictingListUser =
                await _unitOfWork.ListUserRepository.GetWithoutDetailsByEmailAddressAsync(
                    listUserCreateDto.EmailAddress, ct);

            if (conflictingListUser is not null)
                return new(false, null, true, conflictingListUser);

            var newUserId = await _unitOfWork.ListUserRepository.CreateAsync(listUserCreateDto, ct);

            var checkResult = await _unitOfWork.SaveChangesAsync(ct);

            if (checkResult != 1)
                return new(false, null, false, null);

            return new(true, newUserId, false, null);
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(ListUserService), nameof(CheckConflictAndCreateUserAsync));
            throw numberedException;
        }
    }

    public async Task<UpdateRestrictedRecordResult<ListUser?>> CheckAccessAndUpdateNameAsync(
        Guid requestingUserId, Guid listUserId, ListUserPatchDto listUserPatchDto, CancellationToken ct = default)
    {
        try
        {
            if (requestingUserId != listUserId)
                return new(null, false, false, null, null);

            var listUser = await _unitOfWork.ListUserRepository.GetWithoutDetailsByIdAsync(listUserId, ct);

            if (listUser is null)
                return new(false, false, true, null, null);

            _unitOfWork.ListUserRepository.UpdateName(listUser, listUserPatchDto);

            var checkResult = await _unitOfWork.SaveChangesAsync(ct);

            if (checkResult != 1)
                return new(true, false, true, null, null);

            return new(true, true, true, null, null);
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(ListUserService), nameof(CheckAccessAndUpdateNameAsync));
            throw numberedException;
        }
    }

    public async Task<UpdateRestrictedRecordResult<ListUser?>> CheckAccessAndUpdatePasswordAsync(Guid requestingUserId,
        Guid listUserId,
        string newPasswordHash, CancellationToken ct = default)
    {
        try
        {
            if (requestingUserId != listUserId)
                return new(null, false, false, null, null);

            var listUser = await _unitOfWork.ListUserRepository.GetWithoutDetailsByIdAsync(listUserId, ct);

            if (listUser is null)
                return new(false, false, true, null, null);

            _unitOfWork.ListUserRepository.UpdatePassword(listUser, newPasswordHash);


            var checkResult = await _unitOfWork.SaveChangesAsync(ct);

            if (checkResult != 1)
                return new(true, false, true, null, null);

            return new(true, true, true, null, null);
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(ListUserService), nameof(CheckAccessAndUpdatePasswordAsync));
            throw numberedException;
        }
    }

    public async Task<RemoveRestrictedRecordResult> CheckAccessAndDeleteUserAsync(Guid requestingUserId,
        Guid listUserId, CancellationToken ct = default)
    {
        try
        {
            if (requestingUserId != listUserId)
                return new(null, false, false, 0);

            var listUser = await _unitOfWork.ListUserRepository.GetWithoutDetailsByIdAsync(listUserId, ct);

            if (listUser is null)
                return new(false, true, false, 0);

            var deleteResult = await DeleteUserAndCascadeAsync(listUser, ct);

            return new(true, true, deleteResult.Success, deleteResult.RecordsAffected);
        }
        catch (NumberedException)
        {
            throw;
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(ListUserService), nameof(CheckAccessAndDeleteUserAsync));
            throw numberedException;
        }
    }

    public async Task<UpdateRecordResult<object?>> CheckExistenceAndExpireUserAsAdminAsync(Guid userId)
    {
        try
        {
            var targetUser = await _unitOfWork.ListUserRepository.GetWithoutDetailsByIdAsync(userId);

            if (targetUser is null)
                return new(false, false, false, null);

            _unitOfWork.ListUserRepository.SetExpirationDateTime(targetUser, DateTimeOffset.UtcNow);

            var checkResult = await _unitOfWork.SaveChangesAsync();

            if (checkResult != 1)
                return new(true, false, true, null);

            return new(true, true, true, null);
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(ListUserService), nameof(CheckExistenceAndExpireUserAsAdminAsync));
            throw numberedException;
        }
    }

    public async Task<RemoveRecordResult> CheckExistenceAndDeleteUserAsAppAdminAsync(Guid listUserId,
        CancellationToken ct = default)
    {
        try
        {
            var listUser = await _unitOfWork.ListUserRepository.GetWithoutDetailsByIdAsync(listUserId, ct);

            if (listUser is null)
                return new(false, true, 0);

            return await DeleteUserAndCascadeAsync(listUser, ct);
        }
        catch (NumberedException)
        {
            throw;
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(ListUserService), nameof(CheckExistenceAndDeleteUserAsAppAdminAsync));
            throw numberedException;
        }
    }

    private async Task<RemoveRecordResult> DeleteUserAndCascadeAsync(ListUser listUser, CancellationToken ct = default)
    {
        try
        {
            // start transaction

            await _unitOfWork.BeginTransactionAsync(ct);

            // get lists not owned by user and delete their memberships

            var collaboratorLists =
                await _unitOfWork.ListMembershipRepository.GetAllListMembershipsWithDetailsForNotOwnerByUserAsync(
                    listUser.UserId,
                    ct);
            var recordsToBeRemoved = collaboratorLists.Count;

            foreach (var listMembership in collaboratorLists)
            {
                _unitOfWork.ListMembershipRepository.Delete(listMembership);
            }

            // get lists owned by user

            var ownerMemberships =
                await _unitOfWork.ListMembershipRepository.GetAllListMembershipsWithDetailsForOwnerByUserAsync(
                    listUser.UserId, ct);

            foreach (var ownerMembership in ownerMemberships)
            {
                // get shopping List
                var targetShoppingList = ownerMembership.ShoppingList!;
                recordsToBeRemoved++; // for the list itself

                // delete its items
                var items =
                    await _unitOfWork.ItemRepository.GetAllByShoppingListIdAsync(targetShoppingList.ShoppingListId, ct);
                recordsToBeRemoved += items.Count;

                _unitOfWork.ItemRepository.DeleteBatch(items);

                // delete its memberships
                var memberships =
                    await _unitOfWork.ListMembershipRepository
                        .GetAllMembershipsWithoutCascadingInfoByShoppingListIdAsync(
                            targetShoppingList.ShoppingListId, ct);
                recordsToBeRemoved += memberships.Count;

                _unitOfWork.ListMembershipRepository.DeleteBatch(memberships);

                // delete list
                _unitOfWork.ShoppingListRepository.Delete(targetShoppingList);
            }

            // delete email confirmation tokens
            var emailTokens =
                await _unitOfWork.EmailConfirmationTokenRepository.GetAllByUserIdAsync(listUser.UserId,
                    ValidityCheck.None, ct);
            recordsToBeRemoved += emailTokens.Count;

            _unitOfWork.EmailConfirmationTokenRepository.DeleteBatch(emailTokens);

            // delete api keys
            var apiKeys =
                await _unitOfWork.ApiKeyRepository.GetAllByUserIdAsync(listUser.UserId, ValidityCheck.None, ct);
            recordsToBeRemoved += apiKeys.Count;

            _unitOfWork.ApiKeyRepository.DeleteBatch(apiKeys);

            // delete user
            recordsToBeRemoved++;

            _unitOfWork.ListUserRepository.Delete(listUser);

            var checkResult = await _unitOfWork.SaveChangesAsync(ct);

            if (checkResult != recordsToBeRemoved)
            {
                await _unitOfWork.RollbackTransactionAsync(ct);
                return new(false, false, 0);
            }

            await _unitOfWork.CommitTransactionAsync(ct);
            return new(true, true, checkResult);
        }
        catch (Exception e)
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(ListUserService), nameof(DeleteUserAndCascadeAsync));
            throw numberedException;
        }
    }
}