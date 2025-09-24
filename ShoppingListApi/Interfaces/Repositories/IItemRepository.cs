using ShoppingListApi.Model.DTOs.Patch;
using ShoppingListApi.Model.DTOs.Post;
using ShoppingListApi.Model.Entity;

namespace ShoppingListApi.Interfaces.Repositories;

public interface IItemRepository
{
    /// <summary>
    /// Retrieves an item by its ID and shopping list ID.
    /// Returns null if not found.
    /// </summary>
    Task<Item?> GetByIdAsync(Guid shoppingListId, Guid itemId, CancellationToken ct = default);

    /// <summary>
    /// Retrieves all items for a given shopping list ID.
    /// </summary>
    Task<List<Item>> GetAllByShoppingListIdAsync(Guid shoppingListId, CancellationToken ct = default);
    
    /// <summary>
    /// Creates a new item in the specified shopping list and returns its ID.
    /// Does not save changes to the database.
    /// </summary>
    Task<Guid> CreateAsync(Guid shoppingListId, ItemPostDto itemPostDto, CancellationToken ct = default);

    /// <summary>
    /// Updates the specified item with new values from the patch DTO.
    /// </summary>
    void UpdateById(Item targetItem, ItemPatchDto itemPatchDto);

    /// <summary>
    /// Removes the specified item from the database context. Does not save changes.
    /// </summary>
    void Delete(Item targetItem);

    /// <summary>
    /// Removes a batch of items from the database context. Does not save changes.
    /// </summary>
    void DeleteBatch(List<Item> items);
}