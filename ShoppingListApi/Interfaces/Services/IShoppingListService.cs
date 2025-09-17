using ShoppingListApi.Interfaces.Repositories;
using ShoppingListApi.Model.DTOs.Get;
using ShoppingListApi.Model.DTOs.Post;
using ShoppingListApi.Model.Entity;
using ShoppingListApi.Model.ReturnTypes;

namespace ShoppingListApi.Interfaces.Services;

public interface IShoppingListService
{
    IShoppingListRepository ShoppingListRepository { get; }

    Task<FetchRestrictedRecordResult<List<ShoppingListGetDto>?>> CheckAccessAndGetAllShoppingListsForUser(
        IListMembershipService listMembershipService, Guid requestingUserId, Guid userId,
        CancellationToken ct = default);

    Task<FetchRestrictedRecordResult<ShoppingListGetDto?>> CheckAccessAndGetShoppingListByIdAsync(
        IListMembershipService listMembershipService, Guid requestingUserId, Guid userId, Guid shoppingListId,
        CancellationToken ct = default);

    Task<ShoppingListAdditionResult> CheckConflictAndCreateShoppingListAsync(
        IListMembershipService listMembershipService, IUserRoleService userRoleService, Guid requestingUserId,
        Guid userId,
        ShoppingListPostDto shoppingListPostDto,
        CancellationToken ct = default);

    Task<UpdateRestrictedRecordResult<ShoppingList?>> CheckAccessAndUpdateShoppingListNameAsync(
        IListMembershipService listMembershipService, Guid requestingUserId, Guid shoppingListId,
        ShoppingListPostDto shoppingListPostDto, CancellationToken ct = default);

    Task<RemoveRestrictedRecordResult> CheckAccessAndDeleteShoppingListAsync(
        IListMembershipService listMembershipService, Guid requestingUserId, Guid shoppingListId,
        CancellationToken ct = default);

    Task<RemoveRecordResult> CheckExistenceAndDeleteShoppingListAsAppAdminAsync(
        Guid shoppingListId, CancellationToken ct = default);
}