using ShoppingListApi.Model.DTOs.Create;
using ShoppingListApi.Model.DTOs.Patch;
using ShoppingListApi.Model.Entity;
using ShoppingListApi.Model.ReturnTypes;

namespace ShoppingListApi.Interfaces.Repositories;

public interface IListUserRepository
{
    Task<ListUser?> GetWithoutDetailsByIdAsync(Guid listUserId, CancellationToken ct = default);

    Task<ListUser?> GetWithoutDetailsByEmailAddressAsync(string emailAddress, CancellationToken ct = default);
    
    Task<Guid?> CreateAsync(ListUserCreateDto listUserCreateDto, CancellationToken ct = default);
    
    Task<bool> UpdateNameAsync(ListUser listUser, ListUserPatchDto listUserPatchDto, CancellationToken ct = default);
    
    Task<bool> UpdatePassword(ListUser listUser, string newPasswordHash, CancellationToken ct = default);
    
    Task<RemoveRecordResult> DeleteAsync(ListUser listUser, CancellationToken ct = default);
    
    
}