using ShoppingListApi.Interfaces.Repositories;

namespace ShoppingListApi.Interfaces.Services;

public interface IItemService
{
    IItemRepository ItemRepository { get; }
    
    
}