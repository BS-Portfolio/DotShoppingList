using ShoppingListApi.Model.DTOs.Post;
using ShoppingListApi.Model.Entity;

namespace ShoppingListApi.Interfaces.Repositories;

public interface IShoppingListRepository
{
    Task<ShoppingList?> GetWithoutItemsByIdAsync(Guid shoppingListId, CancellationToken ct = default);
    Task<ShoppingList?> GetWithItemsByIdAsync(Guid shoppingListId, CancellationToken ct = default);

    Task<Guid> CreateAsync(ShoppingListPostDto shoppingListPostDto, CancellationToken ct = default);

    void UpdateName(ShoppingList targetShoppingList, ShoppingListPostDto shoppingListPostDto);

    void Delete(ShoppingList targetShoppingList);
    void DeleteBatch(List<ShoppingList> targetShoppingLists);
}