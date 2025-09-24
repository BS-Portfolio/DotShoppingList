using ShoppingListApi.Model.DTOs.Post;
using ShoppingListApi.Model.Entity;

namespace ShoppingListApi.Interfaces.Repositories;

public interface IShoppingListRepository
{
    /// <summary>
    /// Retrieves a ShoppingList by its ID without including related items.
    /// Returns null if not found.
    /// </summary>
    Task<ShoppingList?> GetWithoutItemsByIdAsync(Guid shoppingListId, CancellationToken ct = default);

    /// <summary>
    /// Retrieves a ShoppingList by its ID, including related items.
    /// Returns null if not found.
    /// </summary>
    Task<ShoppingList?> GetWithItemsByIdAsync(Guid shoppingListId, CancellationToken ct = default);

    /// <summary>
    /// Creates a new ShoppingList from the provided DTO and returns its ID.
    /// Does not save changes to the database.
    /// </summary>
    Task<Guid> CreateAsync(ShoppingListPostDto shoppingListPostDto, CancellationToken ct = default);

    /// <summary>
    /// Updates the name of the specified ShoppingList using the provided DTO.
    /// </summary>
    void UpdateName(ShoppingList targetShoppingList, ShoppingListPostDto shoppingListPostDto);

    /// <summary>
    /// Removes the specified ShoppingList from the database context. Does not save changes.
    /// </summary>
    void Delete(ShoppingList targetShoppingList);

    /// <summary>
    /// Removes a batch of ShoppingLists from the database context. Does not save changes.
    /// </summary>
    void DeleteBatch(List<ShoppingList> targetShoppingLists);
}