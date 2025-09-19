using ShoppingListApi.Model.DTOs.Patch;
using ShoppingListApi.Model.DTOs.Post;
using ShoppingListApi.Model.Entity;

namespace ShoppingListApi.Interfaces.Repositories;

public interface IItemRepository
{
    Task<Item?> GetByIdAsync(Guid shoppingListId, Guid itemId, CancellationToken ct = default);

    Task<List<Item>> GetAllByShoppingListIdAsync(Guid shoppingListId, CancellationToken ct = default);
    Task<Guid> CreateAsync(Guid shoppingListId, ItemPostDto itemPostDto, CancellationToken ct = default);

    void UpdateById(Item targetItem, ItemPatchDto itemPatchDto);

    void Delete(Item targetItem);

    void DeleteBatch(List<Item> items);
}