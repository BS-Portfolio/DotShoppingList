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

    public async Task<List<ShoppingListGetDto>> GetAllShoppingListsForUserAsync(
        IShoppingListService shoppingListService, Guid userId,
        CancellationToken ct = default)
    {
        List<ShoppingListGetDto> result = [];

        try
        {
            // get shopping list ids

            var shoppingListIds = await _listMembershipRepository.GetAllShoppingListIdsForUserAsync(userId, ct);

            if (shoppingListIds.Count == 0)
                return result;

            foreach (var shoppingListId in shoppingListIds)
            {
                // get shopping list with items 
                var shoppingListsWithItems =
                    await shoppingListService.ShoppingListRepository.GetWithItemsByIdAsync(shoppingListId, ct);

                if (shoppingListsWithItems is null)
                    throw new Exception("Shopping list with the following Id not found: " + shoppingListId);

                // get list owner
                var shoppingListOwner =
                    await _listMembershipRepository.GetShoppingListOwner(shoppingListId, ct);

                if (shoppingListOwner is null)
                    throw new Exception("Shopping list owner with the following Id not found: " + shoppingListId);

                // get list collaborators
                var shoppingListCollaborators =
                    await _listMembershipRepository.GetShoppingListCollaborators(shoppingListId, ct);

                // map to dto
                var ownerDto = new ListUserMinimalGetDto(
                    shoppingListOwner.UserId,
                    shoppingListOwner.FirstName,
                    shoppingListOwner.LastName,
                    shoppingListOwner.EmailAddress
                );

                var shoppingListGetDto =
                    new ShoppingListGetDto(shoppingListId, shoppingListsWithItems.ShoppingListName, ownerDto);

                if (shoppingListsWithItems.Items.Count > 0)
                {
                    shoppingListGetDto.AddItemsToShoppingList(
                        ItemGetDto.FromItemBatch(shoppingListsWithItems.Items.ToList()));
                }

                if (shoppingListCollaborators.Count > 0)
                {
                    shoppingListGetDto.AddCollaboratorsToShoppingList(
                        ListUserMinimalGetDto.FromListUserBatch(shoppingListCollaborators));
                }
            }

            return result;
        }
        catch (Exception e)
        {
            _logger.LogError("The method {MethodName} failed with exception: {Exception}",
                nameof(GetAllShoppingListsForUserAsync), e.ToString());
            throw;
        }
    }

    public async Task<(bool? AccessGranted, ShoppingListGetDto? shoppingList)>
        CheckAccessAndGetShoppingListForUserAsync(
            IShoppingListService shoppingListService, Guid userId, Guid shoppingListId,
            CancellationToken ct = default)
    {
        try
        {
            // check access
            var hasAccess = await IsOwnerOrCollaboratorAsync(userId, shoppingListId, ct);

            if (hasAccess is false)
                return (false, null!);

            // if access granted get shopping list with items
            var targetShoppingListWithItems =
                await shoppingListService.ShoppingListRepository.GetWithItemsByIdAsync(shoppingListId, ct);

            if (targetShoppingListWithItems is null)
                return (true, null);

            // get list owner
            var shoppingListOwner =
                await _listMembershipRepository.GetShoppingListOwner(shoppingListId, ct);

            if (shoppingListOwner is null)
                return (true, null!);

            // get list collaborators
            var collaborators =
                await _listMembershipRepository.GetShoppingListCollaborators(shoppingListId, ct);

            // map to dto
            var ownerDto = new ListUserMinimalGetDto(shoppingListOwner);

            var shoppingListGetDto =
                new ShoppingListGetDto(shoppingListId, targetShoppingListWithItems.ShoppingListName, ownerDto);

            if (targetShoppingListWithItems.Items.Count > 0)
            {
                var convertedItems = ItemGetDto.FromItemBatch(targetShoppingListWithItems.Items.ToList());
                shoppingListGetDto.AddItemsToShoppingList(convertedItems);
            }

            if (collaborators.Count > 0)
            {
                var convertedCollaborators = ListUserMinimalGetDto.FromListUserBatch(collaborators);
                shoppingListGetDto.AddCollaboratorsToShoppingList(convertedCollaborators);
            }

            return (true, shoppingListGetDto);
        }
        catch (Exception e)
        {
            _logger.LogError("The method {MethodName} failed with exception: {Exception}",
                nameof(CheckAccessAndGetShoppingListForUserAsync), e.ToString());
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
        IUserRoleRepository userRoleRepository, Guid userId, Guid shoppingListId, UserRoleEnum userRoleEnum,
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

            var userRoleObj = await userRoleRepository.GetByEnumAsync(userRoleEnum, ct);

            if (userRoleObj is null)
                throw new Exception("User role not found for enum: " + userRoleEnum);

            // assign user to shopping list
            var assignmentSuccess = await _listMembershipRepository
                .AssignUserToShoppingListByUserRoleIdAsync(userId, shoppingListId, userRoleObj.UserRoleId, ct);

            if (assignmentSuccess is false)
                return new(false, null, false, null);

            var addedListMemberShip = new ListMembership()
            {
                ShoppingListId = shoppingListId,
                UserId = userId,
                UserRoleId = userRoleObj.UserRoleId
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