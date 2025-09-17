using ShoppingListApi.Interfaces.Repositories;
using ShoppingListApi.Model.DTOs.Create;
using ShoppingListApi.Model.DTOs.Patch;
using ShoppingListApi.Model.DTOs.Post;
using ShoppingListApi.Model.Entity;
using ShoppingListApi.Model.ReturnTypes;

namespace ShoppingListApi.Interfaces.Services;

public interface IListUserService
{
    IListUserRepository ListUserRepository { get; }

    Task<AddRecordResult<Guid?, ListUser?>> CheckConflictAndCreateUserAsync(ListUserCreateDto listUserCreateDto,
        CancellationToken ct = default);

    Task<UpdateRestrictedRecordResult<ListUser?>> CheckAccessAndUpdateNameAsync(Guid requestingUserId, Guid listUserId,
        ListUserPatchDto listUserPatchDto,
        CancellationToken ct = default);

    Task<UpdateRestrictedRecordResult<ListUser?>> CheckAccessAndUpdatePasswordAsync(
        Guid requestingUserId, Guid listUserId, string newPasswordHash,
        CancellationToken ct = default);

    Task<RemoveRestrictedRecordResult> CheckAccessAndDeleteUserAsync(Guid requestingUserId, Guid listUserId,
        CancellationToken ct = default);

    Task<RemoveRecordResult>
        CheckExistenceAndDeleteUserAsAppAdminAsync(Guid listUserId, CancellationToken ct = default);
    
    
}