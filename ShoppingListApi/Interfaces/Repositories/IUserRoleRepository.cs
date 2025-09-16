using ShoppingListApi.Enums;
using ShoppingListApi.Model.DTOs.PatchObsolete;
using ShoppingListApi.Model.DTOs.Post;
using ShoppingListApi.Model.Entity;

namespace ShoppingListApi.Interfaces.Repositories;

public interface IUserRoleRepository
{
    Task<List<UserRole>> GetAllAsync(CancellationToken ct = default);
    Task<UserRole?> GetByIdAsync(Guid userRoleId, CancellationToken ct = default);
    Task<UserRole?> GetByEnumAsync(UserRoleEnum userRoleEnum, CancellationToken ct = default);

    Task<UserRole?> GetByTitleAsync(string userRoleTitle, CancellationToken ct = default);

    Task<Guid?> AddAsync(UserRolePostDto userRolePostDto, CancellationToken ct = default);
    Task<bool> UpdateAsync(Guid userRoleId, UserRolePatchDtoObsolete userRolePatchDtoObsolete, CancellationToken ct = default);
}