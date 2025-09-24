using ShoppingListApi.Model.Entity;
using ShoppingListApi.Model.ReturnTypes;

namespace ShoppingListApi.Interfaces.Services;

public interface IListMembershipService
{
    /// <summary>
    /// Adds a collaborator to a shopping list as the list owner, checking for user existence, role, and conflicts.
    /// Returns an AddCollaboratorResult with state flags.
    /// </summary>
    Task<AddCollaboratorResult> AddCollaboratorToShoppingListAsListOwnerAsync(
        Guid requestingUserId, string collaboratorEmailAddress, Guid shoppingListId, CancellationToken ct = default);

    /// <summary>
    /// Assigns a user to a shopping list as an admin, checking for existing membership conflicts.
    /// Returns an AddRecordResult with state flags and the new or conflicting membership.
    /// </summary>
    Task<AddRecordResult<ListMembership?, ListMembership?>> AssignUserToShoppingListAsAdminAsync(
        Guid userRoleId, Guid userId, Guid shoppingListId,
        CancellationToken ct = default);

    /// <summary>
    /// Removes a user from a shopping list as an application admin, after ensuring dependencies are removed.
    /// Returns a RemoveRecordResult with state flags and affected records count.
    /// </summary>
    Task<RemoveRecordResult> RemoveUserFromShoppingListAsApplicationAdminAsync(Guid userId, Guid shoppingListId,
        CancellationToken ct = default);

    /// <summary>
    /// Removes a collaborator from a shopping list as the list owner, checking for owner self-removal and user role.
    /// Returns a RemoveRestrictedRecordResult with state flags and affected records count.
    /// </summary>
    Task<RemoveRestrictedRecordResult> RemoveCollaboratorFromShoppingListAsListOwnerAsync(
        Guid requestingUserId, Guid collaboratorUserId, Guid shoppingListId, CancellationToken ct = default);

    /// <summary>
    /// Allows a collaborator to leave a shopping list, checking for user role and membership existence.
    /// Returns a RemoveRestrictedRecordResult with state flags and affected records count.
    /// </summary>
    Task<RemoveRestrictedRecordResult> LeaveShoppingListAsCollaboratorAsync(Guid requestingUserId,
        Guid collaboratorUserId, Guid shoppingListId, CancellationToken ct = default);
}