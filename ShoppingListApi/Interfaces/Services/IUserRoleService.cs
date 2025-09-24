using ShoppingListApi.Model.DTOs.Patch;
using ShoppingListApi.Model.DTOs.Post;
using ShoppingListApi.Model.Entity;
using ShoppingListApi.Model.ReturnTypes;

namespace ShoppingListApi.Interfaces.Services;

public interface IUserRoleService
{
    /// <summary>
    /// Retrieves the user role entity for the list owner.
    /// </summary>
    Task<UserRole?> GetOwnerUserRole(CancellationToken ct = default);

    /// <summary>
    /// Retrieves the user role entity for a collaborator.
    /// </summary>
    Task<UserRole?> GetCollaboratorUserRole(CancellationToken ct = default);

    /// <summary>
    /// Retrieves a user role entity by its unique identifier.
    /// </summary>
    Task<UserRole?> GetByIdAsync(Guid userRoleId, CancellationToken ct = default);

    /// <summary>
    /// Retrieves all user role entities.
    /// </summary>
    Task<List<UserRole>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Adds a new user role if no conflict exists; returns conflict info if a role with the same enum or title exists.
    /// </summary>
    Task<AddRecordResult<Guid?, UserRole?>> CheckConflictAndAddUserRoleAsync(
        UserRolePostDto userRolePostDto, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing user role if no conflict exists; returns conflict info if a role with the same enum or title exists.
    /// </summary>
    Task<UpdateRecordResult<UserRole?>> CheckConflictAndUpdateUserRoleAsync(
        Guid userRoleId, UserRolePatchDto userRolePatchDto, CancellationToken ct = default);
}