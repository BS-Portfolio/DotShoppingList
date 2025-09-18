using ShoppingListApi.Interfaces.Repositories;
using ShoppingListApi.Model.DTOs.Get;
using ShoppingListApi.Model.DTOs.Post;
using ShoppingListApi.Model.Entity;
using ShoppingListApi.Model.ReturnTypes;

namespace ShoppingListApi.Interfaces.Services;

public interface IShoppingListService
{
    Task<FetchRestrictedRecordResult<List<ShoppingListGetDto>?>> CheckAccessAndGetAllShoppingListsForUser(
        Guid requestingUserId, Guid userId, CancellationToken ct = default);

    Task<FetchRestrictedRecordResult<ShoppingListGetDto?>> CheckAccessAndGetShoppingListByIdAsync(
        Guid requestingUserId, Guid userId, Guid shoppingListId, CancellationToken ct = default);

    Task<ShoppingListAdditionResult> CheckConflictAndCreateShoppingListAsync(
        Guid requestingUserId, Guid userId, ShoppingListPostDto shoppingListPostDto,
        CancellationToken ct = default);

    Task<UpdateRestrictedRecordResult<ShoppingList?>> CheckAccessAndUpdateShoppingListNameAsync(
        Guid requestingUserId, Guid shoppingListId, ShoppingListPostDto shoppingListPostDto,
        CancellationToken ct = default);

    Task<RemoveRestrictedRecordResult> CheckAccessAndDeleteShoppingListAsync(
        Guid requestingUserId, Guid shoppingListId, CancellationToken ct = default);

    Task<RemoveRecordResult> CheckExistenceAndDeleteShoppingListAsAppAdminAsync(
        Guid shoppingListId, CancellationToken ct = default);
}