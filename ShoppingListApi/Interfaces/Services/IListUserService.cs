using ShoppingListApi.Model.DTOs.Create;
using ShoppingListApi.Model.DTOs.Get;
using ShoppingListApi.Model.DTOs.Patch;
using ShoppingListApi.Model.Entity;
using ShoppingListApi.Model.ReturnTypes;

namespace ShoppingListApi.Interfaces.Services;

public interface IListUserService
{
    /// <summary>
    /// Retrieves a ListUser with all related details by email address.
    /// Returns null if not found.
    /// </summary>
    Task<ListUser?> GetWitDetailsByEmailAddressAsync(string emailAddress,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves a ListUser with all related details by user ID.
    /// Returns null if not found.
    /// </summary>
    Task<ListUser?> GetWithDetailsByIdAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Retrieves all users as minimal DTOs.
    /// </summary>
    Task<List<ListUserMinimalGetDto>> GetAllUsersAsync(CancellationToken ct = default);

    /// <summary>
    /// Creates a new ListUser, checking for email conflicts.
    /// Returns the result including the new user ID or conflicting user if one exists.
    /// </summary>
    Task<AddRecordResult<Guid?, ListUser?>> CheckConflictAndCreateUserAsync(ListUserCreateDto listUserCreateDto,
        CancellationToken ct = default);

    /// <summary>
    /// Updates the name of a ListUser, checking access and existence.
    /// Returns an UpdateRestrictedRecordResult with state flags.
    /// </summary>
    Task<UpdateRestrictedRecordResult<ListUser?>> CheckAccessAndUpdateNameAsync(Guid requestingUserId,
        ListUserPatchDto listUserPatchDto,
        CancellationToken ct = default);

    /// <summary>
    /// Updates the password of a ListUser, checking access and existence.
    /// Returns an UpdateRestrictedRecordResult with state flags.
    /// </summary>
    Task<UpdateRestrictedRecordResult<ListUser?>> CheckAccessAndUpdatePasswordAsync(
        Guid requestingUserId, string newPasswordHash,
        CancellationToken ct = default);

    /// <summary>
    /// Deletes a ListUser, checking access and existence.
    /// Returns a RemoveRestrictedRecordResult with state flags and affected records count.
    /// </summary>
    Task<RemoveRestrictedRecordResult> CheckAccessAndDeleteUserAsync(Guid requestingUserId,
        CancellationToken ct = default);

    /// <summary>
    /// Deletes a ListUser as an app admin, checking existence.
    /// Returns a RemoveRecordResult with state flags and affected records count.
    /// </summary>
    Task<RemoveRecordResult>
        CheckExistenceAndDeleteUserAsAppAdminAsync(Guid listUserId, CancellationToken ct = default);

    /// <summary>
    /// Expires a ListUser as an admin, checking existence.
    /// Returns an UpdateRecordResult with state flags.
    /// </summary>
    Task<UpdateRecordResult<object?>> CheckExistenceAndExpireUserAsAdminAsync(Guid userId);
}