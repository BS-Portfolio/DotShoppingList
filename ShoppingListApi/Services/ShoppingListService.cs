using ShoppingListApi.Configs;
using ShoppingListApi.Enums;
using ShoppingListApi.Exceptions;
using ShoppingListApi.Interfaces.Services;
using ShoppingListApi.Model.DTOs.Get;
using ShoppingListApi.Model.DTOs.Post;
using ShoppingListApi.Model.Entity;
using ShoppingListApi.Model.ReturnTypes;

namespace ShoppingListApi.Services;

public class ShoppingListService(
    IUnitOfWork unitOfWork,
    ILogger<ShoppingListService> logger,
    IConfiguration configuration) : IShoppingListService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<ShoppingListService> _logger = logger;
    private readonly IConfiguration _configuration = configuration;

    public async Task<FetchRestrictedRecordResult<List<ShoppingListGetDto>?>> CheckAccessAndGetAllShoppingListsForUser(
        Guid requestingUserId, CancellationToken ct = default)
    {
        List<ShoppingListGetDto> result = [];

        try
        {
            var userShoppingListMemberships =
                await _unitOfWork.ListMembershipRepository.GetAllMembershipsWithCascadingInfoByUserIdAsync(
                    requestingUserId, ct);

            if (userShoppingListMemberships.Count == 0)
                return new(result, true, false);

            var shoppingListIds = userShoppingListMemberships.Select(lm => lm.ShoppingListId).ToList();

            var shoppingListMembers = await
                _unitOfWork.ListMembershipRepository.GetAllMembershipsWithCascadingInfoByShoppingListIdsAsync(
                    shoppingListIds, ct);

            foreach (var membership in userShoppingListMemberships)
            {
                // get list owner
                var shoppingListOwner =
                    shoppingListMembers.FirstOrDefault(slm =>
                            slm.ShoppingListId == membership.ShoppingListId &&
                            slm.UserRole!.EnumIndex == (int)UserRoleEnum.ListOwner)
                        ?.User;

                if (shoppingListOwner is null)
                    throw new Exception("Shopping list owner with the following Id not found: " +
                                        membership.ShoppingListId);

                // get list collaborators
                var shoppingListCollaborators =
                    shoppingListMembers.Where(slm =>
                            slm.ShoppingListId == membership.ShoppingListId &&
                            slm.UserRole!.EnumIndex != (int)UserRoleEnum.ListOwner)
                        .Select(slm => slm.User!).ToList();

                // map to dto
                var ownerDto = new ListUserMinimalGetDto(
                    shoppingListOwner.UserId,
                    shoppingListOwner.FirstName,
                    shoppingListOwner.LastName,
                    shoppingListOwner.EmailAddress
                );

                var shoppingListGetDto =
                    new ShoppingListGetDto(membership.ShoppingListId, membership.ShoppingList!.ShoppingListName,
                        ownerDto);

                var shoppingListItemsList = membership.ShoppingList.Items.ToList();

                if (shoppingListItemsList.Count > 0)
                {
                    shoppingListGetDto.AddItemsToShoppingList(
                        ItemGetDto.FromItemBatch(shoppingListItemsList));
                }

                if (shoppingListCollaborators.Count > 0)
                {
                    shoppingListGetDto.AddCollaboratorsToShoppingList(
                        ListUserMinimalGetDto.FromListUserBatch(shoppingListCollaborators));
                }

                // add to result
                result.Add(shoppingListGetDto);
            }

            return new(result, true, true);
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(ShoppingListService), nameof(CheckAccessAndGetAllShoppingListsForUser));
            throw numberedException;
        }
    }

    public async Task<FetchRestrictedRecordResult<ShoppingListGetDto?>> CheckAccessAndGetShoppingListByIdAsync(
        Guid requestingUserId, Guid shoppingListId, CancellationToken ct = default)
    {
        try
        {
            var userRole =
                await _unitOfWork.ListMembershipRepository.GetUserRoleObjInShoppingListAsync(requestingUserId,
                    shoppingListId,
                    ct);

            if (userRole is null)
                return new(null, false);

            // if access granted get shopping list with items
            var targetShoppingListWithItems =
                await _unitOfWork.ShoppingListRepository.GetWithItemsByIdAsync(shoppingListId, ct);

            if (targetShoppingListWithItems is null)
                return new(null, true, false);

            var shoppingListMemberships =
                await _unitOfWork.ListMembershipRepository.GetAllMembershipsWithCascadingInfoByShoppingListIdAsync(
                    shoppingListId, ct);

            // get list owner
            var shoppingListOwner =
                shoppingListMemberships.FirstOrDefault(slm =>
                        slm.ShoppingListId == shoppingListId &&
                        slm.UserRole!.EnumIndex == (int)UserRoleEnum.ListOwner)
                    ?.User;

            if (shoppingListOwner is null)
                throw new Exception("Shopping list owner with the following Id not found: " +
                                    shoppingListId);

            // get list collaborators
            var shoppingListCollaborators =
                shoppingListMemberships.Where(slm =>
                        slm.ShoppingListId == shoppingListId &&
                        slm.UserRole!.EnumIndex != (int)UserRoleEnum.ListOwner)
                    .Select(slm => slm.User!).ToList();


            // map to dto
            var ownerDto = new ListUserMinimalGetDto(shoppingListOwner);

            var shoppingListGetDto =
                new ShoppingListGetDto(shoppingListId, targetShoppingListWithItems.ShoppingListName, ownerDto);

            if (targetShoppingListWithItems.Items.Count > 0)
            {
                var convertedItems = ItemGetDto.FromItemBatch(targetShoppingListWithItems.Items.ToList());
                shoppingListGetDto.AddItemsToShoppingList(convertedItems);
            }

            if (shoppingListCollaborators.Count > 0)
            {
                var convertedCollaborators = ListUserMinimalGetDto.FromListUserBatch(shoppingListCollaborators);
                shoppingListGetDto.AddCollaboratorsToShoppingList(convertedCollaborators);
            }

            return new(shoppingListGetDto, true, true);
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(ShoppingListService), nameof(CheckAccessAndGetShoppingListByIdAsync));
            throw numberedException;
        }
    }


    /// <summary>
    /// Don't forget the authorization check before calling the function!
    /// </summary>
    /// <param name="requestingUserId"></param>
    /// <param name="shoppingListPostDto"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<ShoppingListAdditionResult> CheckConflictAndCreateShoppingListAsync(
        Guid requestingUserId, ShoppingListPostDto shoppingListPostDto, CancellationToken ct = default)
    {
        try
        {
            var maxShoppingListsPerUser =
                _configuration.GetValue<int>("ShoppingLists_MaxAmount");

            if (maxShoppingListsPerUser <= 0)
                maxShoppingListsPerUser = 5;

            var ownerListMemberships =
                await _unitOfWork.ListMembershipRepository.GetAllListMembershipsWithDetailsForOwnerByUserAsync(requestingUserId,
                    ct);

            if (ownerListMemberships.Count >= maxShoppingListsPerUser)
                return new(false, null, true, null, null, true);

            if ((ownerListMemberships.Select(sl => sl.ShoppingList!.ShoppingListName).ToList()
                    .Contains(shoppingListPostDto.ShoppingListName, StringComparer.OrdinalIgnoreCase)))
                return new(false, null, false, null, true, true);

            var ownerUserRole = await _unitOfWork.UserRoleRepository.GetByEnumAsync(UserRoleEnum.ListOwner, ct);

            if (ownerUserRole is null)
                return new(false, null, false, null, false, true);

            // create and assign in a sql transaction

            await _unitOfWork.BeginTransactionAsync(ct);

            var newShoppingListId = await _unitOfWork.ShoppingListRepository.CreateAsync(shoppingListPostDto, ct);

            var checkNewShoppingListAddResult = await _unitOfWork.SaveChangesAsync(ct);

            if (checkNewShoppingListAddResult != 1)
            {
                await _unitOfWork.RollbackTransactionAsync(ct);
                return new(false, null, false, null, false, true);
            }

            await _unitOfWork.ListMembershipRepository.AssignUserToShoppingListByUserRoleIdAsync(requestingUserId,
                newShoppingListId, ownerUserRole.UserRoleId, ct);

            var checkListMembershipAddResult = await _unitOfWork.SaveChangesAsync(ct);

            if (checkListMembershipAddResult != 1)
            {
                await _unitOfWork.RollbackTransactionAsync(ct);
                return new(false, null, false, false, false, true);
            }

            await _unitOfWork.CommitTransactionAsync(ct);

            return new(true, newShoppingListId, false, true, false, true);
        }
        catch (Exception e)
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(ShoppingListService), nameof(CheckConflictAndCreateShoppingListAsync));
            throw numberedException;
        }
    }

    public async Task<UpdateRestrictedRecordResult<ShoppingList?>> CheckAccessAndUpdateShoppingListNameAsync(
        Guid requestingUserId, Guid shoppingListId, ShoppingListPostDto shoppingListPostDto,
        CancellationToken ct = default)
    {
        try
        {
            var userRole =
                await _unitOfWork.ListMembershipRepository.GetUserRoleEnumInShoppingListAsync(requestingUserId,
                    shoppingListId, ct);

            if (userRole is not UserRoleEnum.ListOwner)
                return new(null, false, false, null, null);

            var targetShoppingList =
                await _unitOfWork.ShoppingListRepository.GetWithoutItemsByIdAsync(shoppingListId, ct);

            if (targetShoppingList is null)
                return new(false, false, true, null, null);

            var conflictingShoppingList =
                await _unitOfWork.ListMembershipRepository.OwnsShoppingListWithNameAsync(requestingUserId,
                    shoppingListPostDto.ShoppingListName, shoppingListId, ct);


            if (conflictingShoppingList is not null)
                return new(true, false, true, true, conflictingShoppingList);

            _unitOfWork.ShoppingListRepository.UpdateName(targetShoppingList, shoppingListPostDto);

            var checkResult = await _unitOfWork.SaveChangesAsync(ct);

            if (checkResult != 1)
                return new(true, false, true, null, null);

            return new(true, true, true, false, null);
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(ShoppingListService), nameof(CheckAccessAndUpdateShoppingListNameAsync));
            throw numberedException;
        }
    }

    public async Task<RemoveRestrictedRecordResult> CheckAccessAndDeleteShoppingListAsync(
        Guid requestingUserId, Guid shoppingListId, CancellationToken ct = default)
    {
        try
        {
            var userRole =
                await _unitOfWork.ListMembershipRepository.GetUserRoleEnumInShoppingListAsync(requestingUserId,
                    shoppingListId, ct);

            if (userRole is not UserRoleEnum.ListOwner)
                return new(null, false, false, 0);

            var deletionResult = await DeleteShoppingListByIdAndCascadeAsync(shoppingListId, ct);

            return new(deletionResult.TargetExists, true, deletionResult.Success, deletionResult.RecordsAffected);
        }
        catch (NumberedException)
        {
            throw;
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(ShoppingListService), nameof(CheckAccessAndDeleteShoppingListAsync));
            throw numberedException;
        }
    }

    public Task<RemoveRecordResult> CheckExistenceAndDeleteShoppingListAsAppAdminAsync(Guid shoppingListId,
        CancellationToken ct = default)
    {
        return DeleteShoppingListByIdAndCascadeAsync(shoppingListId, ct);
    }


    private async Task<RemoveRecordResult> DeleteShoppingListByIdAndCascadeAsync(Guid shoppingListId,
        CancellationToken ct = default)
    {
        try
        {
            var recordsToBeRemoved = 0;

            // get shopping list
            var targetShoppingList = await _unitOfWork.ShoppingListRepository.GetWithItemsByIdAsync(shoppingListId, ct);

            if (targetShoppingList is null)
                return new(false, false, 0);

            // update counter
            recordsToBeRemoved += 1;

            // start transaction
            await _unitOfWork.BeginTransactionAsync(ct);

            // delete items 
            if (targetShoppingList.Items.Count > 0)
                recordsToBeRemoved += targetShoppingList.Items.Count;

            _unitOfWork.ItemRepository.DeleteBatch(targetShoppingList.Items.ToList());

            // get list memberships
            var listMemberships =
                await _unitOfWork.ListMembershipRepository.GetAllMembershipsWithoutCascadingInfoByShoppingListIdAsync(
                    shoppingListId, ct);

            // delete memberships
            if (listMemberships.Count > 0)
                recordsToBeRemoved += listMemberships.Count;

            _unitOfWork.ListMembershipRepository.DeleteBatch(listMemberships);

            // delete shopping list
            _unitOfWork.ShoppingListRepository.Delete(targetShoppingList);

            var checkResult = await _unitOfWork.SaveChangesAsync(ct);

            if (checkResult != recordsToBeRemoved)
            {
                await _unitOfWork.RollbackTransactionAsync(ct);
                return new(false, false, 0);
            }

            await _unitOfWork.CommitTransactionAsync(ct);

            return new(true, true, checkResult);
        }
        catch (Exception e)
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(ShoppingListService), nameof(DeleteShoppingListByIdAndCascadeAsync));
            throw numberedException;
        }
    }
}