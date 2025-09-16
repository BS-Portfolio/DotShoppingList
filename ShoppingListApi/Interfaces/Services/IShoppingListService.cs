using ShoppingListApi.Interfaces.Repositories;

namespace ShoppingListApi.Interfaces.Services;

public interface IShoppingListService
{
    IShoppingListRepository ShoppingListRepository { get; }
}