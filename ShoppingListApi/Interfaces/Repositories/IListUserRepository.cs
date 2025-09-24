using ShoppingListApi.Model.DTOs.Create;
using ShoppingListApi.Model.DTOs.Patch;
using ShoppingListApi.Model.Entity;

namespace ShoppingListApi.Interfaces.Repositories;

public interface IListUserRepository
{
    /// <summary>
    /// Retrieves a ListUser by its ID without including related entities.
    /// Returns null if not found.
    /// </summary>
    Task<ListUser?> GetWithoutDetailsByIdAsync(Guid listUserId, CancellationToken ct = default);

    /// <summary>
    /// Retrieves a ListUser by its email address without including related entities.
    /// Returns null if not found.
    /// </summary>
    Task<ListUser?> GetWithoutDetailsByEmailAddressAsync(string emailAddress, CancellationToken ct = default);

    /// <summary>
    /// Retrieves a ListUser by its ID, including related ApiKeys, EmailConfirmationTokens, ListMemberships, ShoppingLists, and UserRoles.
    /// Returns null if not found.
    /// </summary>
    Task<ListUser?> GetWithDetailsByIdAsync(Guid listUserId, CancellationToken ct = default);

    /// <summary>
    /// Retrieves a ListUser by its email address, including related ApiKeys, EmailConfirmationTokens, ListMemberships, ShoppingLists, and UserRoles.
    /// Returns null if not found.
    /// </summary>
    Task<ListUser?> GetWithDetailsByEmailAddressAsync(string emailAddress, CancellationToken ct = default);

    /// <summary>
    /// Retrieves all ListUsers without including related entities.
    /// </summary>
    Task<List<ListUser>> GetAllWithoutDetailsAsync(CancellationToken ct = default);

    /// <summary>
    /// Creates a new ListUser from the provided DTO and returns its ID.
    /// Does not save changes to the database.
    /// </summary>
    Task<Guid> CreateAsync(ListUserCreateDto listUserCreateDto, CancellationToken ct = default);

    /// <summary>
    /// Updates the first and/or last name of the specified ListUser using the provided patch DTO.
    /// </summary>
    void UpdateName(ListUser listUser, ListUserPatchDto listUserPatchDto);

    /// <summary>
    /// Sets the expiration date/time for the specified ListUser.
    /// </summary>
    void SetExpirationDateTime(ListUser listUser, DateTimeOffset? expirationDateTime);

    /// <summary>
    /// Updates the password hash of the specified ListUser.
    /// </summary>
    void UpdatePassword(ListUser listUser, string newPasswordHash);

    /// <summary>
    /// Removes the specified ListUser from the database context. Does not save changes.
    /// </summary>
    void Delete(ListUser listUser);
}