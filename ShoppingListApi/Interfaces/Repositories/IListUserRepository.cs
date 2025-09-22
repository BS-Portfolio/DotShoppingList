using ShoppingListApi.Model.DTOs.Create;
using ShoppingListApi.Model.DTOs.Patch;
using ShoppingListApi.Model.Entity;

namespace ShoppingListApi.Interfaces.Repositories;

public interface IListUserRepository
{
    Task<ListUser?> GetWithoutDetailsByIdAsync(Guid listUserId, CancellationToken ct = default);

    Task<ListUser?> GetWithoutDetailsByEmailAddressAsync(string emailAddress, CancellationToken ct = default);
    Task<ListUser?> GetWithDetailsByIdAsync(Guid listUserId, CancellationToken ct = default);

    Task<ListUser?> GetWithDetailsByEmailAddressAsync(string emailAddress, CancellationToken ct = default);
    Task<List<ListUser>> GetAllWithoutDetailsAsync(CancellationToken ct = default);
    Task<Guid> CreateAsync(ListUserCreateDto listUserCreateDto, CancellationToken ct = default);

    void UpdateName(ListUser listUser, ListUserPatchDto listUserPatchDto);
    void SetExpirationDateTime(ListUser listUser, DateTimeOffset? expirationDateTime);
    void UpdatePassword(ListUser listUser, string newPasswordHash);

    void Delete(ListUser listUser);
}