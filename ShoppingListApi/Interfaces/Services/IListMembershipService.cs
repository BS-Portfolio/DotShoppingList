using ShoppingListApi.Enums;
using ShoppingListApi.Interfaces.Repositories;
using ShoppingListApi.Model.DTOs.Get;
using ShoppingListApi.Model.Entity;
using ShoppingListApi.Model.ReturnTypes;

namespace ShoppingListApi.Interfaces.Services;

public interface IListMembershipService
{
    Task<AddCollaboratorResult> AddCollaboratorToShoppingListAsListOwnerAsync(
        Guid requestingUserId, string collaboratorEmailAddress, Guid shoppingListId, CancellationToken ct = default);

    Task<AddRecordResult<ListMembership?, ListMembership?>> AssignUserToShoppingListAsAdminAsync(
        Guid userRoleId, Guid userId, Guid shoppingListId,
        CancellationToken ct = default);

    Task<RemoveRecordResult> RemoveUserFromShoppingListAsApplicationAdminAsync(Guid userId, Guid shoppingListId,
        CancellationToken ct = default);

    Task<RemoveRestrictedRecordResult> RemoveCollaboratorFromShoppingListAsListOwnerAsync(
        Guid requestingUserId, Guid collaboratorUserId, Guid shoppingListId, CancellationToken ct = default);

    Task<RemoveRestrictedRecordResult> LeaveShoppingListAsCollaboratorAsync(Guid requestingUserId,
        Guid collaboratorUserId, Guid shoppingListId, CancellationToken ct = default);
}