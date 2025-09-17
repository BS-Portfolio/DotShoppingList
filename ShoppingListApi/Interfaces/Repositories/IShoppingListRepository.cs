using ShoppingListApi.Interfaces.Services;
using ShoppingListApi.Model.DTOs.Post;
using ShoppingListApi.Model.Entity;
using ShoppingListApi.Model.ReturnTypes;

namespace ShoppingListApi.Interfaces.Repositories;

public interface IShoppingListRepository
{
    Task<ShoppingList?> GetWithoutItemsByIdAsync(Guid shoppingListId, CancellationToken ct = default);
    Task<ShoppingList?> GetWithItemsByIdAsync(Guid shoppingListId, CancellationToken ct = default);

    Task<Guid?> CreateAsync(ShoppingListPostDto shoppingListPostDto, CancellationToken ct = default);

    Task<Guid?> CreateAndAssignInTransactionAsync(IListMembershipService listMembershipService,
        Guid userId, ShoppingListPostDto shoppingListPostDto, Guid ownerUserRoleId, CancellationToken ct = default);

    Task<bool> UpdateNameAsync(ShoppingList targetShoppingList, ShoppingListPostDto shoppingListPostDto,
        CancellationToken ct = default);

    /// <summary>
    /// Deletes the specified shopping list in a transaction.
    /// If there are related entities (like items or memberships), they will also be deleted in the transaction.
    /// </summary>
    /// <param name="targetShoppingList"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<RemoveRecordResult> DeleteAndCascadeAsync(ShoppingList targetShoppingList, CancellationToken ct = default);

    Task<bool> DeleteAndCascadeByIdAsync(Guid targetShoppingListId, CancellationToken ct = default);
}