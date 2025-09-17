using ShoppingListApi.Interfaces.Repositories;
using ShoppingListApi.Interfaces.Services;
using ShoppingListApi.Model.DTOs.Create;
using ShoppingListApi.Model.DTOs.Patch;
using ShoppingListApi.Model.DTOs.Post;
using ShoppingListApi.Model.Entity;
using ShoppingListApi.Model.ReturnTypes;

namespace ShoppingListApi.Services;

public class ListUserService(IListUserRepository listUserRepository, ILogger<ListUserService> logger) : IListUserService
{
    private readonly IListUserRepository _listUserRepository = listUserRepository;

    private readonly ILogger _logger = logger;
    public IListUserRepository ListUserRepository => _listUserRepository;

    public async Task<AddRecordResult<Guid?, ListUser?>> CheckConflictAndCreateUserAsync(
        ListUserCreateDto listUserCreateDto,
        CancellationToken ct = default)
    {
        try
        {
            var conflictingListUser =
                await _listUserRepository.GetWithoutDetailsByEmailAddressAsync(listUserCreateDto.EmailAddress, ct);

            if (conflictingListUser is not null)
                return new(false, null, true, conflictingListUser);

            var newUserId = await _listUserRepository.CreateAsync(listUserCreateDto, ct);

            if (newUserId is null)
                return new(false, null, false, null);

            return new(true, newUserId, false, null);
        }
        catch (Exception e)
        {
             _logger.LogError("The method {MethodName} failed with exception: {Exception}",
                nameof(CheckConflictAndCreateUserAsync), e.ToString());
            throw;
        }
    }

    public async Task<UpdateRestrictedRecordResult<ListUser?>> CheckAccessAndUpdateNameAsync(
        Guid requestingUserId, Guid listUserId, ListUserPatchDto listUserPatchDto, CancellationToken ct = default)
    {
        try
        {
            if (requestingUserId != listUserId)
                return new(null, false, false, null, null);

            var listUser = await _listUserRepository.GetWithoutDetailsByIdAsync(listUserId, ct);

            if (listUser is null)
                return new(false, false, true, null, null);

            var updateSuccess = await _listUserRepository.UpdateNameAsync(listUser, listUserPatchDto, ct);

            if (updateSuccess is false)
                return new(true, false, true, null, null);

            return new(true, true, true, null, null);
        }
        catch (Exception e)
        {
             _logger.LogError("The method {MethodName} failed with exception: {Exception}",
                nameof(CheckAccessAndUpdateNameAsync), e.ToString());
            throw;
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

            var listUser = await _listUserRepository.GetWithoutDetailsByIdAsync(listUserId, ct);

            if (listUser is null)
                return new(false, false, true, null, null);

            var updateSuccess = await _listUserRepository.UpdatePassword(listUser, newPasswordHash, ct);

            if (updateSuccess is false)
                return new(true, false, true, null, null);

            return new(true, true, true, null, null);
        }
        catch (Exception e)
        {
             _logger.LogError("The method {MethodName} failed with exception: {Exception}",
                nameof(CheckAccessAndUpdatePasswordAsync), e.ToString());
            throw;
        }
    }

    public async Task<RemoveRestrictedRecordResult> CheckAccessAndDeleteUserAsync(Guid requestingUserId,
        Guid listUserId, CancellationToken ct = default)
    {
        try
        {
            if (requestingUserId != listUserId)
                return new(null, false, false, 0);

            var listUser = await _listUserRepository.GetWithoutDetailsByIdAsync(listUserId, ct);

            if (listUser is null)
                return new(false, true, false, 0);

            var deleteResult = await _listUserRepository.DeleteAsync(listUser, ct);

            return new(true, true, deleteResult.Success, deleteResult.RecordsAffected);
        }
        catch (Exception e)
        {
             _logger.LogError("The method {MethodName} failed with exception: {Exception}",
                nameof(CheckAccessAndDeleteUserAsync), e.ToString());
            throw;
        }
    }

    public async Task<RemoveRecordResult> CheckExistenceAndDeleteUserAsAppAdminAsync(Guid listUserId,
        CancellationToken ct = default)
    {
        try
        {
            var listUser = await _listUserRepository.GetWithoutDetailsByIdAsync(listUserId, ct);

            if (listUser is null)
                return new(false, true, 0);

            return await _listUserRepository.DeleteAsync(listUser, ct);
        }
        catch (Exception e)
        {
             _logger.LogError("The method {MethodName} failed with exception: {Exception}",
                nameof(CheckExistenceAndDeleteUserAsAppAdminAsync), e.ToString());
            throw;
        }
    }
}