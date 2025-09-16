using ShoppingListApi.Interfaces.Repositories;

namespace ShoppingListApi.Interfaces.Services;

public interface IListUserService
{
    IListUserRepository ListUserRepository { get; }
}