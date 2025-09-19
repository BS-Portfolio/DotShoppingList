using ShoppingListApi.Configs;
using ShoppingListApi.Enums;
using ShoppingListApi.Exceptions;
using ShoppingListApi.Interfaces.Services;
using ShoppingListApi.Model.Entity;
using ShoppingListApi.Model.ReturnTypes;

namespace ShoppingListApi.Services;

public class ListMembershipService(IUnitOfWork unitOfWork, ILogger<ListMembershipService> logger)
    : IListMembershipService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    private readonly ILogger<ListMembershipService> _logger = logger;


    public async Task<AddRecordResult<ListMembership?, ListMembership?>> AssignUserToShoppingListAsync(
        Guid userRoleId, Guid userId, Guid shoppingListId, CancellationToken ct = default)
    {
        try
        {
            // check if user is already assigned to the shopping list
            var conflictingMembership =
                await _unitOfWork.ListMembershipRepository.GetListMembershipByCompositePkAsync(shoppingListId, userId,
                    ct);

            if (conflictingMembership is not null)
                return new(false, null, true, conflictingMembership);

            // assign user to shopping list
            var newMembership = await _unitOfWork.ListMembershipRepository
                .AssignUserToShoppingListByUserRoleIdAsync(userId, shoppingListId, userRoleId, ct);

            var checkResult = await _unitOfWork.SaveChangesAsync(ct);

            if (checkResult != 1)
                return new(false, null, false, null);

            return new(true, newMembership, false, null);
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(ListMembershipService), nameof(AssignUserToShoppingListAsync));
            throw;
        }
    }

    /// <summary>
    /// Make sure the dependencies are removed first (e.g. items assigned to the user in the shopping list)
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="shoppingListId"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<RemoveRecordResult> RemoveUserFromShoppingListAsApplicationAdminAsync(Guid userId,
        Guid shoppingListId,
        CancellationToken ct = default)
    {
        try
        {
            var targetListMembership = await _unitOfWork.ListMembershipRepository
                .GetListMembershipByCompositePkAsync(shoppingListId, userId, ct);

            if (targetListMembership is null)
                return new(false, false, 0);

            _unitOfWork.ListMembershipRepository.Delete(targetListMembership);

            var checkResult = await _unitOfWork.SaveChangesAsync(ct);

            if (checkResult != 1)
                return new(true, false, 0);

            return new(true, true, 1);
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(ListMembershipService), nameof(RemoveUserFromShoppingListAsApplicationAdminAsync));
            throw;
        }
    }

    /// <summary>
    /// Check if the owner user wants to delete themselves. If so, reject the operation.
    /// </summary>
    /// <param name="requestingUserId"></param>
    /// <param name="collaboratorUserId"></param>
    /// <param name="shoppingListId"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<RemoveRestrictedRecordResult> RemoveCollaboratorFromShoppingListAsListOwnerAsync(
        Guid requestingUserId, Guid collaboratorUserId, Guid shoppingListId, CancellationToken ct = default)
    {
        try
        {
            if (requestingUserId == collaboratorUserId)
                return new(null, false, false, 0);

            var requestingUserRole =
                await _unitOfWork.ListMembershipRepository.GetUserRoleEnumInShoppingListAsync(requestingUserId,
                    shoppingListId, ct);

            if (requestingUserRole is not UserRoleEnum.ListOwner)
                return new(null, false, false, 0);

            var targetListMembership = await _unitOfWork.ListMembershipRepository
                .GetListMembershipByCompositePkAsync(shoppingListId, collaboratorUserId, ct);

            if (targetListMembership is null)
                return new(false, true, false, 0);

            _unitOfWork.ListMembershipRepository.Delete(targetListMembership);

            var checkResult = await _unitOfWork.SaveChangesAsync(ct);

            if (checkResult != 1)
                return new(true, true, false, 0);

            return new(true, true, true, 1);
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(ListMembershipService), nameof(RemoveCollaboratorFromShoppingListAsListOwnerAsync));
            throw;
        }
    }

    public async Task<RemoveRestrictedRecordResult> LeaveShoppingListAsCollaboratorAsync(Guid requestingUserId,
        Guid collaboratorUserId, Guid shoppingListId, CancellationToken ct = default)
    {
        try
        {
            if (requestingUserId != collaboratorUserId)
                return new(null, false, false, 0);

            var requestingUserRole =
                await _unitOfWork.ListMembershipRepository.GetUserRoleEnumInShoppingListAsync(requestingUserId,
                    shoppingListId, ct);

            if (requestingUserRole is not UserRoleEnum.Collaborator)
                return new(null, false, false, 0);

            var targetListMembership = await _unitOfWork.ListMembershipRepository
                .GetListMembershipByCompositePkAsync(shoppingListId, collaboratorUserId, ct);

            if (targetListMembership is null)
                return new(false, true, false, 0);

            _unitOfWork.ListMembershipRepository.Delete(targetListMembership);

            var checkResult = await _unitOfWork.SaveChangesAsync(ct);

            if (checkResult != 1)
                return new(true, true, false, 0);

            return new(true, true, true, 1);
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(ListMembershipService), nameof(LeaveShoppingListAsCollaboratorAsync));
            throw;
        }
    }
}