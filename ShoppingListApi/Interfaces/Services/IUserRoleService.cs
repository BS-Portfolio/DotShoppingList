using ShoppingListApi.Interfaces.Repositories;
using ShoppingListApi.Model.DTOs.Patch;
using ShoppingListApi.Model.DTOs.Post;
using ShoppingListApi.Model.Entity;
using ShoppingListApi.Model.ReturnTypes;

namespace ShoppingListApi.Interfaces.Services;

public interface IUserRoleService
{
    IUserRoleRepository USerRoleRepository { get; }

    Task<AddRecordResult<Guid?, UserRole?>> CheckConflictAndAddUserRoleAsync(
        UserRolePostDto userRolePostDto, CancellationToken ct = default);

    Task<UpdateRecordResult<UserRole?>> CheckConflictAndUpdateUserRoleAsync(
        Guid userRoleId, UserRolePatchDto userRolePatchDto, CancellationToken ct = default);
}