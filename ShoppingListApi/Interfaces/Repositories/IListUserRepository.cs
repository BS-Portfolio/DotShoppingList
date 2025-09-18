using ShoppingListApi.Model.DTOs.Create;
using ShoppingListApi.Model.DTOs.Patch;
using ShoppingListApi.Model.Entity;

namespace ShoppingListApi.Interfaces.Repositories;

public interface IListUserRepository
{
    Task<ListUser?> GetWithoutDetailsByIdAsync(Guid listUserId, CancellationToken ct = default);

    Task<ListUser?> GetWithoutDetailsByEmailAddressAsync(string emailAddress, CancellationToken ct = default);

    Task<Guid> CreateAsync(ListUserCreateDto listUserCreateDto, CancellationToken ct = default);

    void UpdateName(ListUser listUser, ListUserPatchDto listUserPatchDto);

    void UpdatePassword(ListUser listUser, string newPasswordHash);

    void Delete(ListUser listUser);
}