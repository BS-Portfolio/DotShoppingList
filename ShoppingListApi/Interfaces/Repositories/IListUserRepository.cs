using ShoppingListApi.Model.Entity;

namespace ShoppingListApi.Interfaces.Repositories;

public interface IListUserRepository
{
    Task<ListUser?> GetByIdAsync(Guid listUserId, CancellationToken ct = default);

    Task<ListUser?> GetByEmailAddressAsync(string emailAddress, CancellationToken ct = default);
}