using ShoppingListApi.Model.DTOs.Post;
using ShoppingListApi.Model.Entity;
using ShoppingListApi.Model.ReturnTypes;

namespace ShoppingListApi.Interfaces.Repositories;

public interface IShoppingListRepository
{
    Task<ShoppingList?> GetWithoutItemsByIdAsync(Guid shoppingListId, CancellationToken ct = default);
    Task<ShoppingList?> GetWithItemsByIdAsync(Guid shoppingListId, CancellationToken ct = default);
    Task<Guid?> CreateAsync(ShoppingListPost shoppingListPost, CancellationToken ct = default);

    Task<bool> UpdateNameAsync(ShoppingList targetShoppingList, ShoppingListPost shoppingListPost,
        CancellationToken ct = default);

    /// <summary>
    /// Deletes the specified shopping list in a transaction.
    /// If there are related entities (like items or memberships), they will also be deleted in the transaction.
    /// </summary>
    /// <param name="targetShoppingList"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<RemoveRecordResult> DeleteAndCascadeAsync(ShoppingList targetShoppingList, CancellationToken ct = default);
}