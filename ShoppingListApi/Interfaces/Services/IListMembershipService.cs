using ShoppingListApi.Enums;
using ShoppingListApi.Interfaces.Repositories;
using ShoppingListApi.Model.DTOs.Get;
using ShoppingListApi.Model.Entity;
using ShoppingListApi.Model.ReturnTypes;

namespace ShoppingListApi.Interfaces.Services;

public interface IListMembershipService
{
    IListMembershipRepository ListMembershipRepository { get; }

    Task<UserRoleEnum?> GetUserRoleInShoppingListAsync(Guid userId, Guid shoppingListId,
        CancellationToken ct = default);

    Task<bool> IsOwnerAsync(Guid userId, Guid shoppingListId, CancellationToken ct = default);

    Task<bool> IsCollaboratorAsync(Guid userId, Guid shoppingListId, CancellationToken ct = default);

    Task<bool> IsOwnerOrCollaboratorAsync(Guid userId, Guid shoppingListId, CancellationToken ct = default);

    Task<bool> IsNoAccessAsync(Guid userId, Guid shoppingListId, CancellationToken ct = default);

    Task<AddRecordResult<ListMembership?, ListMembership?>> AssignUserToShoppingListAsync(
        Guid userRoleId, Guid userId, Guid shoppingListId,
        CancellationToken ct = default);

    Task<RemoveRecordResult> RemoveUserFromShoppingListAsync(Guid userId, Guid shoppingListId,
        CancellationToken ct = default);

    Task<RemoveRestrictedRecordResult> RemoveCollaboratorFromShoppingListAsListOwnerAsync(
        Guid requestingUserId, Guid collaboratorUserId, Guid shoppingListId, CancellationToken ct = default);

    Task<RemoveRestrictedRecordResult> LeaveShoppingListAsCollaboratorAsync(Guid requestingUserId,
        Guid collaboratorUserId, Guid shoppingListId, CancellationToken ct = default);
}