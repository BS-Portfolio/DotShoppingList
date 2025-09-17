using ShoppingListApi.Enums;
using ShoppingListApi.Interfaces.Repositories;
using ShoppingListApi.Interfaces.Services;
using ShoppingListApi.Model.DTOs.Get;
using ShoppingListApi.Model.Entity;
using ShoppingListApi.Model.ReturnTypes;

namespace ShoppingListApi.Services;

public class ListMembershipService(
    IListMembershipRepository listMembershipRepository,
    ILogger<ListMembershipService> logger) : IListMembershipService
{
    private readonly IListMembershipRepository _listMembershipRepository = listMembershipRepository;

    private readonly ILogger<ListMembershipService> _logger = logger;

    public IListMembershipRepository ListMembershipRepository => _listMembershipRepository;

    public async Task<UserRoleEnum?> GetUserRoleInShoppingListAsync(Guid userId, Guid shoppingListId,
        CancellationToken ct = default)
    {
        try
        {
            var userRoleObj =
                await _listMembershipRepository.GetUserRoleInShoppingListAsync(userId, shoppingListId, ct);

            if (userRoleObj is null)
                return null;

            var enumIndexIsValid = Enum.IsDefined(typeof(UserRoleEnum), userRoleObj.EnumIndex);

            if (enumIndexIsValid is false)
                return null;

            return (UserRoleEnum)userRoleObj.EnumIndex;
        }
        catch (Exception e)
        {
            _logger.LogError("The method {MethodName} failed with exception: {Exception}",
                nameof(GetUserRoleInShoppingListAsync), e.ToString());
            throw;
        }
    }

    public async Task<bool> IsOwnerAsync(Guid userId, Guid shoppingListId, CancellationToken ct = default)
    {
        try
        {
            var userRoleEnum = await GetUserRoleInShoppingListAsync(userId, shoppingListId, ct);

            return userRoleEnum is UserRoleEnum.ListOwner;
        }
        catch (Exception e)
        {
            _logger.LogError("The method {MethodName} failed with exception: {Exception}", nameof(IsOwnerAsync),
                e.ToString());
            throw;
        }
    }

    public async Task<bool> IsCollaboratorAsync(Guid userId, Guid shoppingListId, CancellationToken ct = default)
    {
        try
        {
            var userRoleEnum = await GetUserRoleInShoppingListAsync(userId, shoppingListId, ct);

            return userRoleEnum is UserRoleEnum.Collaborator;
        }
        catch (Exception e)
        {
            _logger.LogError("The method {MethodName} failed with exception: {Exception}", nameof(IsCollaboratorAsync),
                e.ToString());
            throw;
        }
    }

    public async Task<bool> IsOwnerOrCollaboratorAsync(Guid userId, Guid shoppingListId, CancellationToken ct = default)
    {
        try
        {
            var userRoleEnum = await GetUserRoleInShoppingListAsync(userId, shoppingListId, ct);

            return userRoleEnum is UserRoleEnum.ListOwner or UserRoleEnum.Collaborator;
        }
        catch (Exception e)
        {
            _logger.LogError("The method {MethodName} failed with exception: {Exception}",
                nameof(IsOwnerOrCollaboratorAsync), e.ToString());
            throw;
        }
    }

    public async Task<bool> IsNoAccessAsync(Guid userId, Guid shoppingListId, CancellationToken ct = default)
    {
        try
        {
            var userRoleEnum = await GetUserRoleInShoppingListAsync(userId, shoppingListId, ct);

            return userRoleEnum is null;
        }
        catch (Exception e)
        {
            _logger.LogError("The method {MethodName} failed with exception: {Exception}", nameof(IsNoAccessAsync),
                e.ToString());
            throw;
        }
    }

    public async Task<AddRecordResult<ListMembership?, ListMembership?>> AssignUserToShoppingListAsync(
        Guid userId, Guid shoppingListId, Guid userRoleId,
        CancellationToken ct = default)
    {
        try
        {
            // check if user is already assigned to the shopping list
            var existingUserRole =
                await _listMembershipRepository.GetUserRoleInShoppingListAsync(userId, shoppingListId, ct);

            if (existingUserRole is not null)
            {
                var conflictingMembership = new ListMembership()
                {
                    ShoppingListId = shoppingListId,
                    UserId = userId,
                    UserRoleId = existingUserRole.UserRoleId
                };

                return new(false, null, true, conflictingMembership);
            }

            // assign user to shopping list
            var assignmentSuccess = await _listMembershipRepository
                .AssignUserToShoppingListByUserRoleIdAsync(userId, shoppingListId, userRoleId, ct);

            if (assignmentSuccess is false)
                return new(false, null, false, null);

            var addedListMemberShip = new ListMembership()
            {
                ShoppingListId = shoppingListId,
                UserId = userId,
                UserRoleId = userRoleId
            };

            return new(true, addedListMemberShip, false, null);
        }
        catch (Exception e)
        {
            _logger.LogError("The method {MethodName} failed with exception: {Exception}",
                nameof(AssignUserToShoppingListAsync), e.ToString());
            throw;
        }
    }

    public async Task<RemoveRecordResult> RemoveUserFromShoppingListAsync(Guid userId, Guid shoppingListId,
        CancellationToken ct = default)
    {
        try
        {
            var targetListMembership = await _listMembershipRepository
                .GetListMembershipByCompositePkAsync(userId, shoppingListId, ct);

            if (targetListMembership is null)
                return new(false, false, 0);

            var removalSuccess = await _listMembershipRepository.RemoveListMembershipAsync(targetListMembership, ct);

            if (removalSuccess is false)
                return new(true, false, 0);

            return new(true, true, 1);
        }
        catch (Exception e)
        {
            _logger.LogError("The method {MethodName} failed with exception: {Exception}",
                nameof(RemoveUserFromShoppingListAsync), e.ToString());
            throw;
        }
    }

    public async Task<RemoveRestrictedRecordResult> RemoveCollaboratorFromShoppingListAsListOwnerAsync(
        Guid requestingUserId, Guid collaboratorUserId, Guid shoppingListId, CancellationToken ct = default)
    {
        try
        {
            var isRequestingUserOwner = await IsOwnerAsync(requestingUserId, shoppingListId, ct);

            if (isRequestingUserOwner is false)
                return new(null, false, false, 0);

            var targetListMembership = await _listMembershipRepository
                .GetListMembershipByCompositePkAsync(collaboratorUserId, shoppingListId, ct);

            if (targetListMembership is null)
                return new(false, true, false, 0);

            var deletionSuccess = await _listMembershipRepository.RemoveListMembershipAsync(targetListMembership, ct);

            if (deletionSuccess is false)
                return new(true, true, false, 0);

            return new(true, true, true, 1);
        }
        catch (Exception e)
        {
            _logger.LogError("The method {MethodName} failed with exception: {Exception}",
                nameof(RemoveCollaboratorFromShoppingListAsListOwnerAsync), e.ToString());
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

            var isRequestingUserCollaborator = await IsCollaboratorAsync(requestingUserId, shoppingListId, ct);

            if (isRequestingUserCollaborator is false)
                return new(null, false, false, 0);

            var targetListMembership = await _listMembershipRepository
                .GetListMembershipByCompositePkAsync(collaboratorUserId, shoppingListId, ct);

            if (targetListMembership is null)
                return new(false, true, false, 0);

            var deletionSuccess = await _listMembershipRepository.RemoveListMembershipAsync(targetListMembership, ct);

            if (deletionSuccess is false)
                return new(true, true, false, 0);

            return new(true, true, true, 1);
        }
        catch (Exception e)
        {
            _logger.LogError("The method {MethodName} failed with exception: {Exception}",
                nameof(LeaveShoppingListAsCollaboratorAsync), e.ToString());
            throw;
        }
    }
}