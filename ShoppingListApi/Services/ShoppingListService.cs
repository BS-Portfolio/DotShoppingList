using ShoppingListApi.Enums;
using ShoppingListApi.Interfaces.Repositories;
using ShoppingListApi.Interfaces.Services;
using ShoppingListApi.Model.DTOs.Get;
using ShoppingListApi.Model.DTOs.Post;
using ShoppingListApi.Model.Entity;
using ShoppingListApi.Model.ReturnTypes;

namespace ShoppingListApi.Services;

public class ShoppingListService(
    IShoppingListRepository shoppingListRepository,
    ILogger<ShoppingListService> logger,
    IConfiguration configuration)
    : IShoppingListService
{
    private readonly IShoppingListRepository _shoppingListRepository = shoppingListRepository;
    private readonly ILogger<ShoppingListService> _logger = logger;
    private readonly IConfiguration _configuration = configuration;

    public IShoppingListRepository ShoppingListRepository => _shoppingListRepository;

    public async Task<FetchRestrictedRecordResult<List<ShoppingListGetDto>?>> CheckAccessAndGetAllShoppingListsForUser(
        IListMembershipService listMembershipService, Guid requestingUserId,
        Guid userId, CancellationToken ct = default)
    {
        List<ShoppingListGetDto> result = [];

        try
        {
            if (userId != requestingUserId)
                return new(null, false, null);

            var shoppingListIds =
                await listMembershipService.ListMembershipRepository.GetAllShoppingListIdsForUserAsync(userId, ct);

            if (shoppingListIds.Count == 0)
                return new(result, true, false);

            foreach (var shoppingListId in shoppingListIds)
            {
                // get shopping list with items 
                var shoppingListsWithItems =
                    await _shoppingListRepository.GetWithItemsByIdAsync(shoppingListId, ct);

                if (shoppingListsWithItems is null)
                    throw new Exception("Shopping list with the following Id not found: " + shoppingListId);

                // get list owner
                var shoppingListOwner =
                    await listMembershipService.ListMembershipRepository.GetShoppingListOwner(shoppingListId, ct);

                if (shoppingListOwner is null)
                    throw new Exception("Shopping list owner with the following Id not found: " + shoppingListId);

                // get list collaborators
                var shoppingListCollaborators =
                    await listMembershipService.ListMembershipRepository.GetShoppingListCollaborators(shoppingListId,
                        ct);

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

                // add to result
                result.Add(shoppingListGetDto);
            }

            return new(result, true, true);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<FetchRestrictedRecordResult<ShoppingListGetDto?>> CheckAccessAndGetShoppingListByIdAsync(
        IListMembershipService listMembershipService, Guid requestingUserId, Guid userId, Guid shoppingListId,
        CancellationToken ct = default)
    {
        try
        {
            // check access
            if (userId != requestingUserId)
                return new(null, false);

            var hasAccess = await listMembershipService.IsOwnerOrCollaboratorAsync(userId, shoppingListId, ct);

            if (hasAccess is false)
                return new(null, false);

            // if access granted get shopping list with items
            var targetShoppingListWithItems =
                await _shoppingListRepository.GetWithItemsByIdAsync(shoppingListId, ct);

            if (targetShoppingListWithItems is null)
                return new(null, true, false);

            // get list owner
            var shoppingListOwner =
                await listMembershipService.ListMembershipRepository.GetShoppingListOwner(shoppingListId, ct);

            if (shoppingListOwner is null)
                return new(null, true, false);

            // get list collaborators
            var collaborators =
                await listMembershipService.ListMembershipRepository.GetShoppingListCollaborators(shoppingListId, ct);

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

            return new(shoppingListGetDto, true, true);
        }
        catch (Exception e)
        {
            _logger.LogError("The method {MethodName} failed with exception: {Exception}",
                nameof(CheckAccessAndGetShoppingListByIdAsync), e.ToString());
            throw;
        }
    }


    /// <summary>
    /// Don't forget the authorization check before calling the function! 
    /// </summary>
    /// <param name="listMembershipService"></param>
    /// <param name="userRoleService"></param>
    /// <param name="requestingUserId"></param>
    /// <param name="userId"></param>
    /// <param name="shoppingListPostDto"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<ShoppingListAdditionResult> CheckConflictAndCreateShoppingListAsync(
        IListMembershipService listMembershipService, IUserRoleService userRoleService, Guid requestingUserId,
        Guid userId,
        ShoppingListPostDto shoppingListPostDto,
        CancellationToken ct = default)
    {
        try
        {
            var maxShoppingListsPerUser =
                _configuration.GetValue<int>("ShoppingLists_MaxAmount");

            if (userId != requestingUserId)
                return new(false, null, null, null, null, false);

            var userOwnedShoppingListsIds =
                await listMembershipService.ListMembershipRepository.GetAllShoppingListsOwnedByUserAsync(userId, ct);

            if (userOwnedShoppingListsIds.Count >= maxShoppingListsPerUser)
                return new(false, null, true, null, null, true);

            if ((userOwnedShoppingListsIds.Select(sl => sl.ShoppingListName).ToList()
                    .Contains(shoppingListPostDto.ShoppingListName, StringComparer.OrdinalIgnoreCase)))
                return new(false, null, false, null, true, true);


            var ownerUserRoleId = await userRoleService.GetOwnerUserRole(ct);

            if (ownerUserRoleId is null)
            {
                _logger.LogError("Owner user role not found in the database. Cannot assign user as list owner.");
                return new(false, null, null, null, null, true);
            }

            // // create and assign in a sql transaction
            //
            //
            //
            // return new(true, (Guid)newShoppingListId, false, true, false, true);
            throw new NotImplementedException();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<UpdateRestrictedRecordResult<ShoppingList?>> CheckAccessAndUpdateShoppingListNameAsync(
        IListMembershipService listMembershipService,
        Guid requestingUserId, Guid shoppingListId, ShoppingListPostDto shoppingListPostDto,
        CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public async Task<RemoveRestrictedRecordResult> CheckAccessAndDeleteShoppingListAsync(
        IListMembershipService listMembershipService, Guid requestingUserId,
        Guid shoppingListId, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<RemoveRecordResult> CheckExistenceAndDeleteShoppingListAsAppAdminAsync(Guid shoppingListId,
        CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}